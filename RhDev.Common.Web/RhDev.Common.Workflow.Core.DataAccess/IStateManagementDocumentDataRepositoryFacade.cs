using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Workflow;
using RhDev.Common.Workflow.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.DataAccess
{
    public interface IStateManagementDocumentDataRepositoryFacade : IAutoregisteredService
    {
        Task WriteHistoryAsync(int workflowInstanceDataId, string userId, string eventTitle, string message, DateTime dtm);
        Task<TaskInfo> DelegateTaskAsync(string delegator, string? delegatedUserHint, StateMachineRuntimeParameters parameters);
        Task<IList<TransitionLogInfo>> GetTransitionInfoAsync(string guid);
        Task DeleteDocumentAsync(int dataStoreId);
        Task DeleteTransitionHistoryAsync(string transitionId);
        Task<IList<WorkflowTask>> GetAllUserAssignedTasksAsync(int workflowfId, string userId, string[] userGroups, WorkflowDatabaseTaskStatus databaseTaskStatus);
        Task<IList<WorkflowTask>> GetAllUserAssignedTasksAsync(string userId, string[] userGroups, WorkflowDatabaseTaskStatus databaseTaskStatus);
    }
}
