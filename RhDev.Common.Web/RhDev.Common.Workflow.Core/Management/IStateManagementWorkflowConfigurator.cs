using RhDev.Common.Workflow.Entities;
using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;

namespace RhDev.Common.Workflow.Management
{
    public interface IStateManagementWorkflowConfigurator : IAutoregisteredService
    {
        Task<List<WorkflowInfo>> GetAllWorkflowInstancesAsync(int documentDataId, bool includeCompleted);
        Task StopWorkflowAsync(WorkflowInfo workflowInfo, string initiatorUserId);
        Task<List<WorkflowHistoryItem>> GetWorkflowInstanceHistoryAsync(WorkflowInfo workflowInfo);
        Task<TaskCompletitionInfo> GetLastTaskCompletitionInfoAsync(WorkflowInfo workflowInfo, string taskGroupId);
        Task<WorkflowInfo> StartWorkflowAsync(WorkflowDocumentIdentifier workflowDocumentIdentifier, WorkflowDefinitionFile workflowDefinition, string initiatorUserId, List<WorkflowStartProperty> properties);
    }
}
