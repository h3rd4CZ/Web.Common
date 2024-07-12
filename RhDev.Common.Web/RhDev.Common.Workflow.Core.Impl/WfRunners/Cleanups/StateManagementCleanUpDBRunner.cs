using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace RhDev.Common.Workflow.Impl.WfRunners.Cleanups
{
    public class StateManagementCleanUpDBRunner : StateManagementClenUpRunnerBase<StateManagementCleanUpDBRunner>, IStateManagementCleanUpRunner
    {
        private readonly IOptionsSnapshot<CommonConfiguration> commonConfiguration;
        private readonly IWorkflowInstanceRepository workflowInstanceRepository;
        private readonly IWorkflowDocumentRepository workflowDocumentRepository;

        public StateManagementCleanUpDBRunner(
            ILogger<StateManagementCleanUpDBRunner> traceLogger,
            IOptionsSnapshot<CommonConfiguration> commonConfiguration,
            IWorkflowInstanceRepository workflowInstanceRepository,
            IWorkflowDocumentRepository workflowDocumentRepository
            ) : base(traceLogger)
        {
            this.commonConfiguration = commonConfiguration;
            this.workflowInstanceRepository = workflowInstanceRepository;
            this.workflowDocumentRepository = workflowDocumentRepository;
        }

        protected async override Task DoWork()
        {
            var nowDate = DateTime.Now;

            var deleteFinishedInstancesAfterDays = commonConfiguration.Value.Workflow.DeleteFinishedInstancesAfterDays;
            var deleteDocumentsDeadline = nowDate.AddDays(-deleteFinishedInstancesAfterDays);
                        
            var allFinishedForDelete 
                = await workflowInstanceRepository
                .ReadAsync(w => w.Finished < deleteDocumentsDeadline);

            traceLogger.LogInformation($"Found {allFinishedForDelete.Count} deadlined workflow instances to delete");

            foreach (var deadInstance in allFinishedForDelete)
            {
                await workflowInstanceRepository.DeleteAsync(deadInstance.Id);

                var document = await workflowDocumentRepository.ReadByIdAsync((int)deadInstance.WorkflowDocumentId, new List<Expression<Func<WorkflowDocument, object>>> { d => d.WorkflowInstances });
                                
                if (document.WorkflowInstances.Count == 0)
                {
                    await workflowDocumentRepository.DeleteAsync(document.Id);
                }
            }
        }
    }
}
