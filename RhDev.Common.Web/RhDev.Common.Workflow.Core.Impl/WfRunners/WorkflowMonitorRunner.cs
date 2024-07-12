using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RhDev.Common.Web.Core;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.Impl.Timer.Queue;
using RhDev.Common.Web.Core.Security;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Workflow.Impl.Actions.Definition;
using RhDev.Common.Workflow.Monitor;
using System.Linq.Expressions;

namespace RhDev.Common.Workflow.Core.Impl.WfRunners
{
    public class WorkflowMonitorRunner : IWorkflowMonitorRunner
    {
        private readonly IBackgroundTaskQueue backgroundTaskQueue;
        private readonly IWorkflowInstanceRepository workflowInstanceRepository;
        private readonly ICentralClockProvider centralClockProvider;
        private readonly IWorkflowRunnerPlanner workflowRunnerPlanner;
        private readonly IOptionsSnapshot<CommonConfiguration> options;
        private readonly ILogger<WorkflowMonitorRunner> logger;

        public WorkflowMonitorRunner(
            IBackgroundTaskQueue backgroundTaskQueue,
            IWorkflowInstanceRepository workflowInstanceRepository,
            ICentralClockProvider centralClockProvider,
            IWorkflowRunnerPlanner workflowRunnerPlanner,
            IOptionsSnapshot<CommonConfiguration> options,
            ILogger<WorkflowMonitorRunner> logger)
        {
            this.backgroundTaskQueue = backgroundTaskQueue;
            this.workflowInstanceRepository = workflowInstanceRepository;
            this.centralClockProvider = centralClockProvider;
            this.workflowRunnerPlanner = workflowRunnerPlanner;
            this.options = options;
            this.logger = logger;
        }

        public async Task RunAsync()
        {
            var clockNow = centralClockProvider.Now();

            var allActiveSystemInstances
                = await workflowInstanceRepository
                .ReadAsync(w => Equals(null, w.Finished) && !w.IsFailed && w.WorkflowStateSystem,
                w => new { w.Id },
                include: new List<Expression<Func<WorkflowInstance, object>>> { w => w.WorkflowDocument });


            foreach (var instance in allActiveSystemInstances) await PlanInstance(instance.Id);
        }

        async Task PlanInstance(int instanceEntityId)
        {
            try
            {
                await backgroundTaskQueue.QueueBackgroundWorkItemAsync(new($"{nameof(WorkflowMonitorRunner)}_{instanceEntityId}", async cancellation =>
                {
                    using (new CurrentIdentity(SystemUserNames.System)) await workflowRunnerPlanner.PlanAsync(instanceEntityId);
                }));

                logger.LogTrace($"Workflow instance with ID : {instanceEntityId} has been enqueued for workflow processing");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occured when enqueuing workflow instance with ID : {instanceEntityId}");
            }
        }
    }
}
