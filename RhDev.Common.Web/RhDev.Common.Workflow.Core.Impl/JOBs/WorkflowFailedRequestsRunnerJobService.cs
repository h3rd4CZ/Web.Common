using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RhDev.Common.Web.Core;
using RhDev.Common.Web.Core.Impl.Timer.Cron;
using RhDev.Common.Web.Core.Security;

namespace RhDev.Common.Workflow.Core.Impl.JOBs
{
    public class WorkflowFailedRequestsRunnerJobService : CronJobService<WorkflowFailedRequestsRunnerJobService>
    {
        protected override LogLevel JobWorkingLogLevel => LogLevel.None;

        public WorkflowFailedRequestsRunnerJobService(
            IScheduleConfig<WorkflowFailedRequestsRunnerJobService> config,
            ILogger<WorkflowFailedRequestsRunnerJobService> logger,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
            : base(config.CronExpression, config.RunOnStart, config.TimeZoneInfo, serviceProvider, logger, configuration) { }

        protected override async Task DoWork(CancellationToken cancellationToken)
        {
            await UseConfigurationRefreshedServiceWithNoScopedContainer<IWorkflowManagementFailRequestRunner>(async svc =>
            {
                using (new CurrentIdentity(SystemUserNames.System)) await svc.RunAsync();
            });
        }
    }
}
