using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.Impl.Host;
using RhDev.Common.Web.Core.Impl.Timer;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Workflow.Core.Impl.JOBs;
using RhDev.Customer.Component.App;
using RhDev.Customer.Component.Core.Impl.Data;

const string SVC_NAME = "Workflow Cleanup Job";

var host = ApplicationHostBuilder.CreateMinimalForWindowsServiceHosted<ApplicationDbContext>((ctx, registry) =>
{
    var cron = ctx
            .Configuration
            .GetRequiredSection(ConfigurationUtils.GetPathConfigurationProperty<CommonConfiguration>(c => c.Workflow.WorkflowCleanupJob.Cron!)).Value;
        
    registry.AddCronJob<WorkflowCleanupJobService>(c =>
    {
        c.TimeZoneInfo = TimeZoneInfo.Local;
        c.CronExpression = cron!;
        c.RunOnStart = true;
    });

}, SVC_NAME,
    useSqlServerConfigurationProvider: true,
    useDbContextFactory: true,
    registrationDefinitions: new[]
    {
        CompositionDefinition.GetDefinition(),
        RhDev.Common.Workflow.Core.Composition.CompositionDefinition.GetDefinition(),
        RhDev.Customer.Component.Core.Impl.TestCompositionDefinition.GetDefinition()
    },
    hostBuilderAction: b => b.AddSerilog(
        fileNameTemplate: "c:\\logs\\jobs\\Common_Workflow\\WorkflowCleanup\\WorkflowCleanup-.txt",
        useSql: true,
        appIdentifier: "Workflow_Cleanup"),
    envVarPrefixes: new[] {
        Constants.SOLUTION_PREF
    });

await host.RunAsync();