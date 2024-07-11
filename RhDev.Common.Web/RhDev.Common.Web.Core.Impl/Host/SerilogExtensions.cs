using Microsoft.AspNetCore.Builder;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Serilog;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Configuration;
using RhDev.Common.Web.Core.Configuration.ConfigEntities;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Configuration;
using System.Data;
using Microsoft.Extensions.Hosting;

namespace RhDev.Common.Web.Core.Impl.Host;

public static class SerilogExtensions
{
    public static void AddSerilog(
        this IHostBuilder builder,
        string? template = default,
        string? fileNameTemplate = default,
        bool useSql = false,
        int sqlBatchLimit = 10, 
        string? connStringKey = default,
        string? tableSchema = default,
        RollingInterval rollingInterval = RollingInterval.Day,
        int? retainedFileCountLimit = 31,
        bool writeToProviders = true,
        string? appIdentifier = default)
    {
        builder.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithUtcTime()
                .Enrich.WithProperty("AppIdentifier", appIdentifier)
                .WriteTo.Async(wt =>
                    wt.File(fileNameTemplate ?? "./log/log-.txt",
                    outputTemplate: template ?? "{Timestamp:dd/MM/yyyy HH:mm:ss} {RequestId} {Level} {SourceContext} {Message:lj}{NewLine}{Exception}",
                    rollingInterval: rollingInterval,
                    retainedFileCountLimit: retainedFileCountLimit))
                .ApplyConfigPreferences(context.Configuration, useSql, sqlBatchLimit, connStringKey, tableSchema)
        , writeToProviders: writeToProviders);
    }

    private static void ApplyConfigPreferences(this LoggerConfiguration serilogConfig, IConfiguration configuration, bool useSql, int sqlBatchLimit, string? connStringKey = default, string? tableSchema = default)
    {
        EnrichWithClientInfo(serilogConfig, configuration);

        if (useSql) WriteToDatabase(serilogConfig, configuration, sqlBatchLimit, connStringKey, tableSchema);
    }

    private static void WriteToDatabase(LoggerConfiguration serilogConfig, IConfiguration configuration, int sqlBatchLimit, string? connStringKey = default, string? tableSchema = default)
    {
        var connString = string.IsNullOrWhiteSpace(connStringKey)
                    ? configuration.GetConnectionString(Constants.DEFAULTCONNECTION_KEY)
                    : configuration.GetValue<string>(connStringKey);

        Guard.StringNotNullOrWhiteSpace(connString);

        WriteToSqlServer(serilogConfig, sqlBatchLimit, connString, tableSchema);
    }

    private static void EnrichWithClientInfo(LoggerConfiguration serilogConfig, IConfiguration configuration)
    {
        var privacySettings
            = configuration.GetSection(ConfigurationUtils.GetPathConfigurationProperty<CommonConfiguration>(c => c.PrivacySettings)).Get<PrivacySettingsConfigurtation>();

        if (privacySettings == null) return;
        if (privacySettings.LogClientIpAddresses) serilogConfig.Enrich.WithClientIp();
        if (privacySettings.LogClientAgents) serilogConfig.Enrich.WithRequestHeader("User-Agent");
    }
    
    private static void WriteToSqlServer(LoggerConfiguration serilogConfig, int sqlBatchLimit, string connectionString, string? loggersSchemaName = default)
    {
        Guard.StringNotNullOrWhiteSpace(connectionString);

        MSSqlServerSinkOptions sinkOpts = new()
        {
            TableName = Constants.LOGGERS_DEFAULT_TABLENAME,
            SchemaName = loggersSchemaName ?? Constants.LOGGERS_DEFAULT_TABLESCHEMA,
            AutoCreateSqlDatabase = false,
            AutoCreateSqlTable = false,
            BatchPostingLimit = sqlBatchLimit
        };

        ColumnOptions columnOpts = new()
        {
            Store = new Collection<StandardColumn>
            {
                StandardColumn.Id,
                StandardColumn.TimeStamp,
                StandardColumn.Level,
                StandardColumn.LogEvent,
                StandardColumn.Exception,
                StandardColumn.Message,
                StandardColumn.MessageTemplate,
                StandardColumn.Properties
            },
            AdditionalColumns = new Collection<SqlColumn>
            {
                new()
                {
                    ColumnName = "ClientIP", PropertyName = "ClientIp", DataType = SqlDbType.NVarChar, DataLength = 64
                },
                new()
                {
                    ColumnName = "UserName", PropertyName = "UserName", DataType = SqlDbType.NVarChar, DataLength = 64
                },
                new()
                {
                    ColumnName = "ClientAgent", PropertyName = "UserAgent", DataType = SqlDbType.NVarChar, DataLength = 1024
                },
                new()
                {
                    ColumnName = "CorrelationId", PropertyName = "RequestId", DataType = SqlDbType.NVarChar,
                    DataLength = 64
                },
                new()
                {
                    ColumnName = "SourceContext", PropertyName = "SourceContext", DataType = SqlDbType.NVarChar,
                    DataLength = 1024
                },
                new()
                {
                    ColumnName = "AppIdentifier", PropertyName = "AppIdentifier", AllowNull = true, DataType = SqlDbType.NVarChar,
                    DataLength = 64
                }
            },
            TimeStamp = { ConvertToUtc = false, ColumnName = "TimeStamp" },
            LogEvent = { DataLength = 32768 }
        };
        columnOpts.PrimaryKey = columnOpts.Id;
        columnOpts.TimeStamp.NonClusteredIndex = true;

        serilogConfig.WriteTo.Async(wt => wt.MSSqlServer(
            connectionString,
            sinkOpts,
            columnOptions: columnOpts
        ));
    }

    public static LoggerConfiguration WithUtcTime(this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        return enrichmentConfiguration.With<UtcTimestampEnricher>();
    }
}


internal class UtcTimestampEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory pf)
    {
        logEvent.AddOrUpdateProperty(pf.CreateProperty("TimeStamp", logEvent.Timestamp.UtcDateTime));
    }
}