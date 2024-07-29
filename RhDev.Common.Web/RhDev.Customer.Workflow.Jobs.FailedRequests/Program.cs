
using Microsoft.VisualBasic;
using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.Impl.Host;
using RhDev.Common.Web.Core.Impl.Timer;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Workflow.Core.Impl.JOBs;
using RhDev.Customer.Component.App.Data;
using RhDev.Customer.Component.Core.Impl.Data;

const string SVC_NAME = "Workflow Failed requests Runner Job";

var host = ApplicationHostBuilder.CreateMinimalForWindowsServiceHosted<ApplicationDbContext>((ctx, registry) =>
{
    var cron = ctx
            .Configuration
            .GetRequiredSection(ConfigurationUtils.GetPathConfigurationProperty<CommonConfiguration>(c => c.Workflow.WorkflowFailedRequestsRunnerJob.Cron!)).Value;

    registry.AddHttpContextAccessor();

    registry.AddCronJob<WorkflowFailedRequestsRunnerJobService>(c =>
    {
        c.TimeZoneInfo = TimeZoneInfo.Local;
        c.CronExpression = cron!;
        c.RunOnStart = true;
    });

}, SVC_NAME,
    commonBuilder: b => b.UseQueueHostedService(50, 1000),
    useSqlServerConfigurationProvider: true,
    useDbContextFactory: true,
    registrationDefinitions: new[]
    {
        CompositionDefinition.GetDefinition(),
        RhDev.Common.Workflow.Core.Composition.CompositionDefinition.GetDefinition(),
        RhDev.Customer.Component.Core.Impl.TestCompositionDefinition.GetDefinition()
    },
    dbSaveChangeInterceptorsTypes: new Type[] { typeof(AuditableEntityInterceptor) },
    hostBuilderAction: b => b.AddSerilog(
        useSql: true,
        appIdentifier: "Workflow_FailedRequests"
        ),
    envVarPrefixes: new[] {
        RhDev.Customer.Component.App.Constants.SOLUTION_PREF
    });

await host.RunAsync();