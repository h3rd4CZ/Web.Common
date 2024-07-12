using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Workflow.Core.Management;

namespace RhDev.Common.Workflow.Impl.WfRunners
{
    public class WorkflowManagementFailRequestRunner : IWorkflowManagementFailRequestRunner
    {
        private readonly ILogger<WorkflowManagementFailRequestRunner> traceLogger;
        private readonly IWorkflowTransitionRequestEvaluator workflowTransitionRequestEvaluator;
        private readonly IWorkflowTransitionRequestRepository workflowTransitionRequestRepository;
        private readonly IOptionsSnapshot<CommonConfiguration> options;

        public WorkflowManagementFailRequestRunner(
            ILogger<WorkflowManagementFailRequestRunner> traceLogger,
            IWorkflowTransitionRequestEvaluator workflowTransitionRequestEvaluator,
            IWorkflowTransitionRequestRepository workflowTransitionRequestRepository,
            IOptionsSnapshot<CommonConfiguration> options)
        {
            this.traceLogger = traceLogger;
            this.workflowTransitionRequestEvaluator = workflowTransitionRequestEvaluator;
            this.workflowTransitionRequestRepository = workflowTransitionRequestRepository;
            this.options = options;
        }

        public async Task RunAsync()
        {
            traceLogger.LogInformation($"{nameof(WorkflowManagementFailRequestRunner)} started at {DateTime.Now}");

            try
            {
                int numOfRepeatOnFailRequest = options.Value.Workflow.NumOfRepeatOfFailRequest;

                var failedItems = await workflowTransitionRequestRepository.GetFailRequestsWithRepetationLessOrEqualThenAsync(numOfRepeatOnFailRequest);

                if(failedItems.Count > 0) traceLogger.LogWarning($"There are {failedItems.Count} failed processed items to repeat.");

                foreach (var failedItem in failedItems)
                {
                    var itemToProcess = failedItem;
                    try
                    {
                        var rollbackInfo = itemToProcess.Payload.RollbackInfo;
                        if(rollbackInfo is null || !rollbackInfo.successfully)
                        {
                            await workflowTransitionRequestEvaluator.RollbackTransitionAsync(failedItem.Id);

                            itemToProcess = await workflowTransitionRequestRepository.ReadByIdAsync(failedItem.Id);
                        }
                                                                        
                        await workflowTransitionRequestEvaluator.EvaluateTransitionAsync(itemToProcess, false);
                    }
                    catch(Exception ex)
                    {
                        traceLogger.LogError(ex, $"An error occured when processing failed request item : {failedItem}");
                    }

                }
            }
            catch (Exception ex)
            {
                traceLogger.LogError(ex, $"An error occured when running {nameof(WorkflowManagementFailRequestRunner)}");
            }

            traceLogger.LogInformation($"{nameof(WorkflowManagementFailRequestRunner)} finished at {DateTime.Now}");
        }
    }

}
