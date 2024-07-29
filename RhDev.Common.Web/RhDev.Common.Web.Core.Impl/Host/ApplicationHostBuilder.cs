using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Lamar.Microsoft.DependencyInjection;
using System.Reflection;
using RhDev.Common.Web.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using RhDev.Common.Web.Core.Impl.Configuration;
using Lamar;
using RhDev.Common.Web.Core.Composition.Factory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using RhDev.Common.Web.Core.DataAccess.Sql.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.DependencyInjection;
using RhDev.Common.Web.Core.Impl.Timer;
using RhDev.Common.Web.Core.Composition.Factory.Definitions;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.Impl.DependencyInjection;

namespace RhDev.Common.Web.Core.Impl.Host
{
    public class ApplicationHostBuilder
    {
        public static IHost CreateMinimalForTest(
            string[] envVarPrefixes = default!,
            ITestOutputHelper testOutputHelper = default,
            IDictionary<Type, (Func<IServiceProvider, object>, ServiceLifetime lifetime)> mocks = default,
            bool useSqlServerConfigurationProvider = false,
            bool useDbContextFactory = false,
            Action<HostBuilderContext, IServiceCollection>? serviceConfiguration = default,
            ContainerRegistrationDefinition[] registrationDefinitions = default!)
        => CreateMinimalForTest<DbContext>(envVarPrefixes, testOutputHelper, useSqlServerConfigurationProvider, useDbContextFactory: useDbContextFactory, mocks, serviceConfiguration, registrationDefinitions);

        public static IHost CreateMinimalForTest<TDBContext>(
            string[] envVarPrefixes = default!,
            ITestOutputHelper testOutputHelper = default!,
            bool useSqlServerConfigurationProvider = false,
            bool useDbContextFactory = false,
            IDictionary<Type, (Func<IServiceProvider, object> f, ServiceLifetime lifetime)> mocks = default!,
            Action<HostBuilderContext, IServiceCollection>? serviceConfiguration = default,
            ContainerRegistrationDefinition[] registrationDefinitions = default!) where TDBContext : DbContext
        {
            var builder = CreateMinimalGenericBuilder<TDBContext>((ctx, logging) =>
            {
                if (testOutputHelper is not null) logging.AddProvider(new XunitLoggerProvider(testOutputHelper));

            }, true, envVarPrefixes: envVarPrefixes, mocks: mocks, registrationDefinitions: registrationDefinitions,
                useDbContextFactory: useDbContextFactory,
                registryConfiguration: serviceConfiguration,
                useSqlServerConfigurationProvider: useSqlServerConfigurationProvider);

            return builder.Build();
        }

        public static IHost CreateMinimalForPowerShell<TDBContext>(
            string[] envVarPrefixes = default!,
            Action<HostBuilderContext, IServiceCollection>? registryConfiguration = default,
            bool useSqlServerConfigurationProvider = false,
            bool useDbContextFactory = false,
            ContainerRegistrationDefinition[] registrationDefinitions = default!) where TDBContext : DbContext
        {
            var builder = CreateMinimalGenericBuilder<TDBContext>(
                (ctx, logging) => logging.AddConsole(),
                true,
                envVarPrefixes: envVarPrefixes,
                registrationDefinitions: registrationDefinitions,
                useSqlServerConfigurationProvider: useSqlServerConfigurationProvider,
                useDbContextFactory: useDbContextFactory,
                registryConfiguration: registryConfiguration);

            return builder.Build();
        }

        public static IHost CreateMinimalForWindowsServiceHosted<TDBContext>(
            Action<HostBuilderContext, IServiceCollection> serviceConfiguration,
            string serviceName,
            string[] envVarPrefixes = default!,
            Action<IHostBuilder> hostBuilderAction = default,
            ContainerRegistrationDefinition[] registrationDefinitions = default!,
            bool dbUseLazyLoadingProxies = false,
            bool useSqlServerConfigurationProvider = false,
            bool useDbContextFactory = false,
            Action<CommonWebRegistrationBuilder>? commonBuilder = default,
            Type[] dbSaveChangeInterceptorsTypes = default!) where TDBContext : DbContext
        {
            Guard.StringNotNullOrWhiteSpace(serviceName, nameof(serviceName));

            var builder = CreateMinimalGenericBuilder<TDBContext>(
                (ctx, logging) =>
                {
                    if (OperatingSystem.IsWindows()) logging.AddEventLog();
                    if (ctx.HostingEnvironment.IsDevelopment()) logging.AddConsole();
                },
                false,
                registrationDefinitions,
                envVarPrefixes,
                serviceConfiguration,
                dbUseLazyLoadingProxies: dbUseLazyLoadingProxies,
                dbSaveChangeInterceptorsTypes: dbSaveChangeInterceptorsTypes,
                useDbContextFactory: useDbContextFactory,
                b: commonBuilder,
                useSqlServerConfigurationProvider: useSqlServerConfigurationProvider);

            builder.UseWindowsService(options => options.ServiceName = serviceName);

            if (hostBuilderAction is not null) hostBuilderAction(builder);

            return builder.Build();
        }

        private static IHostBuilder CreateMinimalGenericBuilder<TDBContext>(
            Action<HostBuilderContext, ILoggingBuilder> loggingBuilder,
            bool useContentRoot,
            ContainerRegistrationDefinition[] registrationDefinitions = default!,
            string[] envVarPrefixes = default!,
            Action<HostBuilderContext, IServiceCollection>? registryConfiguration = default,
            IDictionary<Type, (Func<IServiceProvider, object> f, ServiceLifetime lifetime)>? mocks = default,
            bool dbUseLazyLoadingProxies = false,
            bool useSqlServerConfigurationProvider = false,
            bool useDbContextFactory = false,
            Action<CommonWebRegistrationBuilder>? b = default,
            Type[] dbSaveChangeInterceptorsTypes = default!) where TDBContext : DbContext

        {
            var defaultBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder();

            if (useContentRoot)
            {
                var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                defaultBuilder = defaultBuilder.UseContentRoot(rootPath);
            }

            defaultBuilder = defaultBuilder.ConfigureAppConfiguration(config =>
                {
                    config.AddJsonFile(HostSettings.CONFIG_FILE, true);

                    if (envVarPrefixes is not null)
                    {
                        foreach (var prefix in envVarPrefixes.Where(p => !string.IsNullOrWhiteSpace(p)))
                        {
                            config.AddEnvironmentVariables(prefix);
                        }
                    }

                    var cfgBuilt = config.Build();

                    var commonConfiguration = cfgBuilt.GetSection(CommonConfiguration.Get.Path).Get<CommonConfiguration>();

                    if (useSqlServerConfigurationProvider)
                    {
                        SqlProviderApplicationConfigurationSettings
                        .ConfigureSqlConfigurationProvider(cfgBuilt, config, commonConfiguration is not null ? TimeSpan.FromSeconds(commonConfiguration.OptionsReloadInSeconds) : TimeSpan.FromSeconds(60));
                    }
                })
                .UseLamar((ctx, registry) =>
                {
                    registry.ConfigureDatabase<TDBContext>(
                        ctx.Configuration,
                        useDbContextFactory,
                        useLazyLoadingProxies: dbUseLazyLoadingProxies,
                        databaseSaveChangeInterceptorsTypes: dbSaveChangeInterceptorsTypes);

                    registry.AddCommonOptions();

                    if (registryConfiguration is not null) registryConfiguration(ctx, registry);

                    var builder = IServiceCollectionExtensions.ActivateBuilder(b);

                    registry.AddQueueHostedService(ctx.Configuration, builder);

                    registry.ConfigureAppComposition(registrationDefinitions);

                    if (mocks is not null) ConfigureMocks(registry, mocks);
                })
                .ConfigureLogging((ctx, logging) =>
                {
                    logging.ClearProviders();

                    loggingBuilder(ctx, logging);
                });

            return defaultBuilder;

        }

        private static void ConfigureMocks(ServiceRegistry registry, IDictionary<Type, (Func<IServiceProvider, object> f, ServiceLifetime lifetime)> mocks)
        {
            foreach (var mock in mocks)
            {
                var descriptor =
                       new ServiceDescriptor(
                           mock.Key,
                           mock.Value.f,
                           mock.Value.lifetime
                           );

                registry.Replace(descriptor);

                ApplicationContainerFactory.UpdateUnderlayingContainerConfiguration(descriptor);
            }
        }
    }
}
