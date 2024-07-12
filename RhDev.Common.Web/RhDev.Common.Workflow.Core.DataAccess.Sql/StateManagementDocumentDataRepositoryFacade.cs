using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Workflow;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.PropertyModel.Properties;
using RhDev.Common.Workflow.Security;
using RhDev.Common.Workflow.DataAccess.Sql.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Workflow;

namespace RhDev.Common.Workflow.DataAccess.Sql
{
    public class StateManagementDocumentDataRepositoryFacade : IStateManagementDocumentDataRepositoryFacade
    {
        private readonly IWorkflowDocumentRepository workflowDocumentRepository;
        private readonly IWorkflowTransitionLogRepository workflowTransitionLogRepository;
        private readonly IWorkflowTaskRepository workflowTaskRepository;
        private readonly IWorkflowInstanceHistoryRepository workflowInstanceHistoryRepository;
        private readonly IWorkflowMembershipProvider workflowMembershipProvider;
        private readonly IAsynchronousCache<UserSiteAssigningGroups> siteAssingingGroupCache;
        private readonly IPropertyEvaluator propertyEvaluator;

        public StateManagementDocumentDataRepositoryFacade(
            IWorkflowDocumentRepository workflowDocumentRepository,
            IWorkflowTransitionLogRepository workflowTransitionLogRepository,
            IWorkflowTaskRepository workflowTaskRepository,
            IWorkflowInstanceHistoryRepository WorkflowInstanceHistoryRepository,
            IWorkflowMembershipProvider workflowMembershipProvider,
            IAsynchronousCache<UserSiteAssigningGroups> siteAssingingGroupCache,
            IPropertyEvaluator propertyEvaluator)
        {
            this.workflowDocumentRepository = workflowDocumentRepository;
            this.workflowTransitionLogRepository = workflowTransitionLogRepository;
            this.workflowTaskRepository = workflowTaskRepository;
            workflowInstanceHistoryRepository = WorkflowInstanceHistoryRepository;
            this.workflowMembershipProvider = workflowMembershipProvider;
            this.siteAssingingGroupCache = siteAssingingGroupCache;
            this.propertyEvaluator = propertyEvaluator;
        }

        public async Task WriteHistoryAsync(int workflowInstanceDataId, string userId, string eventTitle, string message, DateTime dtm)
        {
            Guard.NumberMin(workflowInstanceDataId, 1);

            var newHistory = new WorkflowInstanceHistory
            {
                Date = Equals(default(DateTime), dtm) ? DateTime.Now : dtm,
                UserId = userId,
                Event = eventTitle,
                Message = message,
                WorkflowInstanceId = workflowInstanceDataId
            };

            await workflowInstanceHistoryRepository.CreateAsync(newHistory);
        }

        public async Task<TaskInfo> DelegateTaskAsync(string delegator, string? delegatedUserHint, StateMachineRuntimeParameters parameters)
        {
            Guard.StringNotNullOrWhiteSpace(delegator, nameof(delegator));
            Guard.NotNull(parameters, nameof(parameters));
            Guard.NotNull(parameters.Workflow, nameof(parameters.Workflow));
            Guard.NumberMin(parameters.Workflow.DataId, 1, nameof(parameters.Workflow.DataId));
            Guard.NotNull(parameters.DocumentMetadataIdentifier, nameof(parameters.DocumentMetadataIdentifier));
            Guard.NotNull(parameters.DocumentMetadataIdentifier.SectionDesignation, nameof(parameters.DocumentMetadataIdentifier.SectionDesignation));
                       

            var userOperand = new Operand
            {
                DataType = WorkflowDataType.User,
                Text = delegator
            };

            var workflowId = parameters.Workflow.DataId;
                        
            var args = new StateTransitionEventArgs
                (    parameters.DocumentMetadataIdentifier.SectionDesignation,
                     StateManagementCommonTriggerProperties.Empty,
                     string.Empty,
                     string.Empty, new Transition { },
                     parameters.DocumentMetadataIdentifier,
                     (int)workflowId,
                     parameters.UserId,
                     new List<string> { },
                     false
                );

            var value = await propertyEvaluator.EvaluateAsync(userOperand, args);

            if (value is StateManagementUserValue userValue)
            {
                var allActiveTasks = await workflowTaskRepository.ReadAsync(t => t.WorkflowInstanceId == workflowId && t.Status == WorkflowDatabaseTaskStatus.NotStarted);

                var taskToDelegate = default(WorkflowTask);

                if (allActiveTasks.Count == 0) throw new InvalidOperationException("There is no not started task assigned to user");
                else if (allActiveTasks.Count == 1)
                {
                    taskToDelegate = allActiveTasks[0];
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(delegatedUserHint)) throw new InvalidOperationException("When multiple assignees is assigned to task delegate user hint must be defined");

                    taskToDelegate = allActiveTasks.FirstOrDefault(t => t.AssignedTo.Equals(delegatedUserHint));

                    if (Equals(null, taskToDelegate)) throw new InvalidOperationException($"There is no valid assignee for delegated hint user name : {delegatedUserHint}");
                }

                var taskInfo = TaskInfo.FillFrom(taskToDelegate);

                Guard.NotNull(taskToDelegate, nameof(taskToDelegate));

                taskToDelegate.AssignedTo = userValue.Id;
                taskToDelegate.AssigneeType =
                        userValue.IsPermissionGroup
                        ? TaskAssigneeType.PermissionGroup
                        : TaskAssigneeType.User;

                await workflowTaskRepository.UpdateAsync(taskToDelegate);
                                
                return taskInfo;

            }
            else throw new InvalidOperationException($"User {delegator} is not castable as user");
        }

        public async Task<IList<TransitionLogInfo>> GetTransitionInfoAsync(string guid)
        {
            Guard.StringNotNullOrWhiteSpace(guid, nameof(guid));

            var transition = (await workflowTransitionLogRepository.ReadAsync(t => t.TransitionId.Equals(guid))).ToList();

            return transition.Select(TransitionLogInfo.FillFrom).ToList();

        }

        public async Task DeleteDocumentAsync(int dataStoreId)
        {
            Guard.NotDefault(dataStoreId, nameof(dataStoreId));

            await workflowDocumentRepository.DeleteAsync(dataStoreId);
        }

        public async Task<IList<WorkflowTask>> GetAllUserAssignedTasksAsync(
            int workflowfId, 
            string userId, 
            string[] userGroups, WorkflowDatabaseTaskStatus databaseTaskStatus)
        {
            Guard.StringNotNullOrWhiteSpace(userId, nameof(userId));
            Guard.NumberMin(workflowfId, 1);
            var wfIdNotSpecified = workflowfId == default;
            var groups = userGroups ?? new string[0];

            Expression<Func<WorkflowTask, bool>> query =
                t => (
                t.WorkflowInstanceId == workflowfId &&
                t.Status == databaseTaskStatus) &&
                (t.AssignedTo.Equals(userId) || userGroups.Contains(t.AssignedTo));

            var allTasks
                = await workflowTaskRepository
                .ReadAsync(query, include: new List<Expression<Func<WorkflowTask, object>>>
                {
                    t => t.WorkflowInstance,
                    t => t.WorkflowInstance.WorkflowDocument });
                        
            return allTasks;
        }

        public async Task<IList<WorkflowTask>> GetAllUserAssignedTasksAsync(string userId, string[] userGroups, WorkflowDatabaseTaskStatus databaseTaskStatus)
        {
            Guard.StringNotNullOrWhiteSpace(userId, nameof(userId));
            var groups = userGroups ?? new string[0];

            Expression<Func<WorkflowTask, bool>> query =
                t => t.Status == databaseTaskStatus && (t.AssignedTo.Equals(userId) || userGroups.Contains(t.AssignedTo));

            var allTasks
                = await workflowTaskRepository.ReadAsync(query, include: new List<Expression<Func<WorkflowTask, object>>> { t => t.WorkflowInstance, t => t.WorkflowInstance.WorkflowDocument });

            return allTasks;
        }

        public async Task DeleteTransitionHistoryAsync(string transitionId)
        {
            Guard.StringNotNullOrWhiteSpace(transitionId, nameof(transitionId));

            var transitionHistory = await workflowTransitionLogRepository.ReadAsync(l => l.TransitionId.Equals(transitionId));

            if (!Equals(null, transitionHistory))
            {
                foreach (WorkflowTransitionLog transitionLog in transitionHistory)
                {
                    await workflowTransitionLogRepository.DeleteAsync(transitionLog.Id);
                }
            }
        }
    }
}
