using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Workflow.DataAccess;
using RhDev.Common.Workflow.Entities;
using System;
using System.Threading.Tasks;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;

namespace RhDev.Common.Workflow.Impl.WfRunners.Cleanups
{
    public class StateManagementCleanUpRequestQueueRunner : StateManagementClenUpRunnerBase<StateManagementCleanUpRequestQueueRunner>, IStateManagementCleanUpRunner
    {
        private readonly IOptionsSnapshot<CommonConfiguration> options;
        private readonly IWorkflowTransitionRequestRepository workflowTransitionRequestRepository;

        public StateManagementCleanUpRequestQueueRunner(
            ILogger<StateManagementCleanUpRequestQueueRunner> traceLogger,
            IOptionsSnapshot<CommonConfiguration> options,
            IWorkflowTransitionRequestRepository workflowTransitionRequestRepository
            ) : base(traceLogger)
        {
            this.options = options;
            this.workflowTransitionRequestRepository = workflowTransitionRequestRepository;
        }

        protected async override Task DoWork()
        {
            int deadlineDays = options.Value.Workflow.DeleteRequestQueueItemsInDays;

            DateTime deadline = DateTime.Now.AddDays(-deadlineDays);

            var requestsToDel =await workflowTransitionRequestRepository.GetRequestsOlderThenAsync(deadline);

            traceLogger.LogInformation($"There are {requestsToDel.Count} workflow transition requests to delete");

            foreach (WorkflowTransitionRequest transitionRequestEntity in requestsToDel)
            {
                try
                {
                    await workflowTransitionRequestRepository.DeleteAsync(transitionRequestEntity.Id);
                }
                catch (Exception ex)
                {
                    traceLogger.LogError(ex,
                        $"An error occured when removing transition request entity with ID : {transitionRequestEntity.Id}");
                }
            }
        }
    }

}
