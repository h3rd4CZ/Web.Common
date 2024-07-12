using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RhDev.Common.Web.Core.Impl.Timer.Cron;

namespace RhDev.Common.Workflow.Core.Impl.JOBs
{
    public class WorkflowRunnerJobService : CronJobService<WorkflowRunnerJobService>
    {
        protected override LogLevel JobWorkingLogLevel => LogLevel.None;

        public WorkflowRunnerJobService(
            IScheduleConfig<WorkflowRunnerJobService> config,
            ILogger<WorkflowRunnerJobService> logger,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
            : base(config.CronExpression, config.RunOnStart, config.TimeZoneInfo, serviceProvider, logger, configuration) { }

        protected override async Task DoWork(CancellationToken cancellationToken)
        {
            await UseConfigurationRefreshedServiceWithNoScopedContainer<IWorkflowMonitorRunner>(async svc =>
            {
                await svc.RunAsync();
            });
        }
    }
}
