using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Management;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;

namespace RhDev.Common.Workflow
{
    public interface IWorkflowService : IAutoregisteredService
    {
        
        /// <summary>
        /// Removes transtion history from DB according the transition ID
        /// </summary>
        /// <param name="transitionId"></param>
        Task DeleteTransitionHistoryAsync(string transitionId);

        /// <summary>
        /// Return true if a document with unique id is in end state
        /// </summary>
        /// <param name="dataStoreId"></param>
        /// <returns></returns>
        Task<bool> IsDocumentInEndStateAsync(StateMachineRuntimeParameters parameters);

        /// <summary>
        /// Delegates task to another assignee
        /// </summary>
        /// <param name="delegator">name / login of user to delegate to</param>
        /// <param name="delegatedUserHint">Hint user name for cases when task has been assigned to multiple users</param>
        /// <param name="parameters"></param>
        Task<TaskInfo> DelegateTaskAsync(string delegator, string? delegatedUserHint, StateMachineRuntimeParameters parameters);

        /// <summary>
        /// Writes explicit history item
        /// </summary>
        /// <param name="workflowInstanceDataId">Id of running workflow instance we would like history write to</param>
        /// <param name="user"></param>
        /// <param name="eventTitle"></param>
        /// <param name="message"></param>
        /// <param name="dtm">Event date and time, if default value (DateTime.MinValue) current date and time is used</param>
        Task WriteHistoryAsync(int workflowInstanceDataId, string user, string eventTitle, string message, DateTime dtm);

        /// <summary>
        /// Deletes the document from database
        /// </summary>
        /// <param name="dataStoreId"></param>
        Task DeleteDocumentAsync(int dataStoreId);

        /// <summary>
        /// Returns transition log information from database
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task<IList<TransitionLogInfo>> GetTransitionLogInfoAsync(string guid);
                
        /// <summary>
        /// Returns history for specified workflow instance
        /// </summary>
        /// <param name="workflowInfo"></param>
        /// <returns></returns>
        Task<List<WorkflowHistoryItem>> GetWorkflowInstanceHistoryAsync(WorkflowInfo workflowInfo);

        /// <summary>
        /// Returns completition info of last completed task
        /// </summary>
        /// <param name="workflowInfo"></param>
        /// <param name="taskGroupId"></param>
        /// <returns></returns>
        Task<TaskCompletitionInfo> GetLastTaskCompletitionInfoAsync(WorkflowInfo workflowInfo, string taskGroupId);
                
        /// <summary>
        /// Starts workflow by file definition
        /// </summary>
        /// <param name="workflowDocumentIdentifier"></param>
        /// <param name="workflowDefinitionFile"></param>
        /// <param name="initialUserId"></param>
        /// <param name="properties"></param>
        Task<WorkflowInfo> StartWorkflowAsync(WorkflowDocumentIdentifier workflowDocumentIdentifier, WorkflowDefinitionFile workflowDefinitionFile, string initiatorUserId, List<WorkflowStartProperty> properties);

        /// <summary>
        /// Stops workflow instance
        /// </summary>
        /// <param name="workflowInfo"></param>
        /// <param name="initiatorUserId"></param>
        /// <returns></returns>
        Task StopWorkflowAsync(WorkflowInfo workflowInfo, string initiatorUserId);
                
        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentDataId"></param>
        /// <returns></returns>
        Task<List<WorkflowInfo>> GetAllWorkflowInstancesForDocumentDataIdAsync(int documentDataId, bool includeCompleted);
                
        /// <summary>
        /// Completes user assigned task, based on transition task responde type executes transition
        /// </summary>
        /// <param name="stateMachineRuntimeParameters"></param>
        /// <returns></returns>
        Task CompleteTaskAsync(StateMachineRuntimeParameters stateMachineRuntimeParameters);
                
        /// <summary>
        /// Returns all permited transition in current context
        /// </summary>
        /// <param name="stateMachineRuntimeParameters">transition runtime parameters</param>
        /// <returns>All available transition in current context</returns>
        Task<IList<WorkflowTransitionInfo>> GetAllPermittedTransitionsAsync(StateMachineRuntimeParameters stateMachineRuntimeParameters);

        Task EnqueueTransitionRequestAsync(WorkflowTransitionRequestPayload payload, bool async);

    }
}
