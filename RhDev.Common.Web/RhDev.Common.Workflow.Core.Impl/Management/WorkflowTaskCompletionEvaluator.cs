using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute.Core;
using RhDev.Common.Web.Core;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Configuration.StateMachineConfig;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.StateDefinition;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Workflow.DataAccess;
using RhDev.Common.Workflow.DataAccess.SharePoint.Online.Security;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.Management;
using RhDev.Common.Workflow.PropertyModel;
using RhDev.Common.Workflow.PropertyModel.Properties;
using RhDev.Common.Workflow.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Impl.Management
{
    public class WorkflowTaskCompletionEvaluator : IWorkflowTaskCompletionEvaluator
    {
        private readonly IStateManagementDocumentDataRepositoryFacade stateManagementDocumentDataRepositoryFacade;
        private readonly IWorkflowTaskRepository workflowTaskRepository;
        private readonly IWorkflowInstanceRepository workflowInstanceRepository;
        private readonly IPropertyEvaluator propertyEvaluator;
        private readonly IUserInfoValueEvaluator userInfoValueEvaluator;
        private readonly ICentralClockProvider centralClockProvider;
        private readonly ILogger<WorkflowTaskCompletionEvaluator> traceLogger;
        private readonly IWorkflowPropertyManager workflowPropertyManager;
        public WorkflowTaskCompletionEvaluator(
            IStateManagementDocumentDataRepositoryFacade stateManagementDocumentDataRepositoryFacade,
            IWorkflowTaskRepository workflowTaskRepository,
            IWorkflowInstanceRepository workflowInstanceRepository,
            IPropertyEvaluator propertyEvaluator,
            IUserInfoValueEvaluator userInfoValueEvaluator,
            ICentralClockProvider centralClockProvider,
            ILogger<WorkflowTaskCompletionEvaluator> traceLogger,
            IWorkflowPropertyManager workflowPropertyManager)
        {
            this.stateManagementDocumentDataRepositoryFacade = stateManagementDocumentDataRepositoryFacade;
            this.workflowTaskRepository = workflowTaskRepository;
            this.workflowInstanceRepository = workflowInstanceRepository;
            this.propertyEvaluator = propertyEvaluator;
            this.userInfoValueEvaluator = userInfoValueEvaluator;
            this.centralClockProvider = centralClockProvider;
            this.traceLogger = traceLogger;
            this.workflowPropertyManager = workflowPropertyManager;
        }

        public async Task<TaskRespondStatus> CompleteTaskAsync(StateMachineRuntimeParameters stateMachineRuntimeParameters, StateMachine sm, StateDefinition currentStateDefinition, ConfiguredState currentStateConfiguration, Configuration.StateMachineConfig.Transitions.Transition transition)
        {
            Guard.NotNull(sm, nameof(sm));
            Guard.NotNull(currentStateDefinition, nameof(currentStateDefinition));
            Guard.NotNull(currentStateConfiguration, nameof(currentStateConfiguration));
            Guard.NotNull(transition, nameof(transition));

            var userResponded = stateMachineRuntimeParameters.UserData.UserRespondedId;
            var workflowDataId = stateMachineRuntimeParameters.Workflow.DataId;
            var userGroups = stateMachineRuntimeParameters.UserPermissionGroups ?? new List<string> { };

            var resolvedDate = DateTime.Now;

            Guard.NotDefault(userResponded, nameof(userResponded));

            var designation = stateMachineRuntimeParameters.DocumentMetadataIdentifier.SectionDesignation;

            Guard.NotNull(designation, nameof(designation));
                        
            if (string.IsNullOrWhiteSpace(userResponded)) return TaskRespondStatus.NoTasksAssigned;

            
            var allUserAssignedTask = await stateManagementDocumentDataRepositoryFacade.GetAllUserAssignedTasksAsync(
                (int)workflowDataId, 
                userResponded, 
                userGroups.ToArray(), 
                WorkflowDatabaseTaskStatus.NotStarted);
                        
            if (allUserAssignedTask.Count() == 0 && !transition.WithoutPermission) throw new InvalidOperationException($"There are no task assigned to user : {userResponded}");
                        
            var currentStateConfig = sm.UserTransitions.FirstOrDefault(c => c.Code == currentStateDefinition.Code);

            Guard.NotNull(currentStateConfig, nameof(currentStateConfig));

            var triggerVariability = currentStateConfig.Transitions?.Count;

            if (Equals(null, triggerVariability)) triggerVariability = 0;

            Guard.NumberMin(triggerVariability.Value, 1, nameof(triggerVariability));

            var workflow = await workflowInstanceRepository.ReadByIdAsync((int)workflowDataId);
            var currentAssignedTasksGroupId = workflow.CurrentTaskGroupId;

            Guard.NotNull(currentAssignedTasksGroupId, nameof(currentAssignedTasksGroupId));
            if (!currentAssignedTasksGroupId.HasValue) throw new InvalidOperationException("currentAssignedTasksGroupId has no value");
                        
            var args = new StateTransitionEventArgs(
                     stateMachineRuntimeParameters.DocumentMetadataIdentifier.SectionDesignation,
                     StateManagementCommonTriggerProperties.Empty,
                     string.Empty,
                     string.Empty, new Configuration.StateMachineConfig.Transitions.Transition { },
                     stateMachineRuntimeParameters.DocumentMetadataIdentifier,
                     (int)workflowDataId,
                     stateMachineRuntimeParameters.UserId,
                     new List<string> { },
                     false
                );

            foreach (var assignedTask in allUserAssignedTask)
            {
                assignedTask.ResolvedById = userResponded;
                assignedTask.ResolvedOn = resolvedDate;
                assignedTask.UserData = await SerializeUserData(stateMachineRuntimeParameters.UserData, transition, args);
                assignedTask.SelectedTriggerCode = stateMachineRuntimeParameters.UserData.Trigger;
                assignedTask.Status = WorkflowDatabaseTaskStatus.Completed;

                await workflowTaskRepository.UpdateAsync(assignedTask);
            }

            var taskRespondeType = allUserAssignedTask.FirstOrDefault()?.TaskRespondeType ?? WorkflowTaskRespondType.FirstWin;

            switch (taskRespondeType)
            {
                case WorkflowTaskRespondType.FirstWin:
                    {
                        await CompleteTaskGroup(workflowDataId, resolvedDate, currentAssignedTasksGroupId.Value, transition, currentStateConfiguration, args);
                        return TaskRespondStatus.Fire;
                    }
                case WorkflowTaskRespondType.MajorityWin:
                    {
                        var respondeStatus = await CheckMajority(workflowDataId, triggerVariability.Value);

                        if (respondeStatus == TaskRespondStatus.WaitToOthers) return TaskRespondStatus.WaitToOthers;

                        await CompleteTaskGroup (workflowDataId, resolvedDate, currentAssignedTasksGroupId.Value, transition, currentStateConfiguration, args);

                        return TaskRespondStatus.Fire;
                    }
                default: throw new InvalidOperationException($"Task responde type : {taskRespondeType} is not valid");
            }
        }

        private async Task<string> SerializeUserData(StateManagementCommonTriggerProperties userData, Configuration.StateMachineConfig.Transitions.Transition transition, StateTransitionEventArgs args)
        {
            if (Equals(null, userData.TriggerParameters)) return null;

            Guard.NotNull(transition.StateManagementTrigger, nameof(transition.StateManagementTrigger));

            var triggerDefinition = transition.StateManagementTrigger;

            var parametersToSerialize = new List<StateManagementValueData> { };

            foreach (var userParameter in userData.TriggerParameters)
            {
                var parameterDef = triggerDefinition.Parameters?.FirstOrDefault(p => p.PropertyName == userParameter.Name);
                if (!Equals(null, parameterDef))
                {
                    var operand = new Operand { DataType = parameterDef.Type, Text = userParameter.Value };
                    var value = await propertyEvaluator.EvaluateAsync(operand, args);
                    parametersToSerialize.Add(new StateManagementValueData { Name = userParameter.Name, Value = value });
                }
            }

            return StateManagementValue.SerializeCollectionProperties(parametersToSerialize);
        }

        private async Task<TaskRespondStatus> CheckMajority(long workflowDataId, int triggerVariability)
        {
            if (triggerVariability == 1) return TaskRespondStatus.Fire;

            var allCompletedTasks = await workflowTaskRepository.ReadAsync(t => t.WorkflowInstanceId == workflowDataId && t.Status == WorkflowDatabaseTaskStatus.Completed);
            var allCompletedTasksCount = allCompletedTasks.Count;
            var allCompletedTriggerOrderTasks = allCompletedTasks.GroupBy(t => t.SelectedTriggerCode).OrderByDescending(g => g.Count()).ToList();
            var allNonCompletedTasksCount = (await workflowTaskRepository.ReadAsync(t => t.WorkflowInstanceId == workflowDataId && t.Status == WorkflowDatabaseTaskStatus.NotStarted))?.Count;

            if (allCompletedTasksCount == 0) return TaskRespondStatus.WaitToOthers;

            var mostFrequentComletedTrigger = allCompletedTriggerOrderTasks.First().Key;
            var mostFrequentComletedTriggerCount = allCompletedTriggerOrderTasks.First().Count();

            if (allCompletedTriggerOrderTasks.Any(o => !o.Key.Equals(mostFrequentComletedTrigger) &&
                 allNonCompletedTasksCount + o.Count() > mostFrequentComletedTriggerCount) || allNonCompletedTasksCount > mostFrequentComletedTriggerCount) return TaskRespondStatus.WaitToOthers;

            return TaskRespondStatus.Fire;
        }

        private async Task CompleteTaskGroup(long workflowDataId, DateTime resolvedDate, Guid currentAssignedTasksGroupId, Configuration.StateMachineConfig.Transitions.Transition transition, ConfiguredState configuredState, StateTransitionEventArgs args)
        {
            
            var allOpenedTasks = await workflowTaskRepository.ReadAsync(t => t.WorkflowInstanceId == workflowDataId && t.GroupId == currentAssignedTasksGroupId && t.Status == WorkflowDatabaseTaskStatus.NotStarted);
            var alltasks = await workflowTaskRepository.ReadAsync(t => t.WorkflowInstanceId == workflowDataId && t.GroupId == currentAssignedTasksGroupId);

            var completionMail = configuredState.CompletionMail;

            foreach (var openedTask in allOpenedTasks)
            {
                openedTask.ResolvedOn = resolvedDate;
                openedTask.Status = WorkflowDatabaseTaskStatus.Completed;
                await workflowTaskRepository.UpdateAsync(openedTask);
            }

            if (!Equals(null, completionMail)) await NotifyCompletion(alltasks, completionMail, args);

            var workflowInstance = await workflowInstanceRepository.ReadByIdAsync((int)workflowDataId);
            var taskGroupId = workflowInstance.CurrentTaskGroupId;
            workflowInstance.CurrentTaskGroupId = null;
            await workflowInstanceRepository.UpdateAsync(workflowInstance);

            if (!string.IsNullOrWhiteSpace(transition?.SaveTaskGroupId))
            {
                await workflowPropertyManager.SavePropertyAsync(transition.SaveTaskGroupId, new StateManagementTextValue(taskGroupId.ToString()), (int)workflowDataId);
            }
        }

        private async Task NotifyCompletion(IList<WorkflowTask> openedTasks, WorkflowTaskMail mail, StateTransitionEventArgs args)
        {
            Guard.NotNull(mail.Subject, nameof(mail.Subject));
            Guard.NotNull(mail.Subject.Operand, nameof(mail.Subject.Operand));
            Guard.NotNull(mail.Text, nameof(mail.Text));
            Guard.NotNull(mail.Text.Operand, nameof(mail.Text.Operand));

            var usersToNotify = new List<UserInfo>();

            foreach (var openedTask in openedTasks)
            {
                if (openedTask.AssigneeType == TaskAssigneeType.User)
                {
                    var userPrincipal = await userInfoValueEvaluator.EvaluateAsUserAsync(openedTask.AssignedTo, args.Designation);

                    if (userPrincipal is not null) usersToNotify.Add(userPrincipal);
                }
                else
                {
                    var users = await userInfoValueEvaluator.EvaluateAsUsersAsync(openedTask.AssignedTo, args.Designation);

                    usersToNotify.AddRange(users);
                }
            }

            usersToNotify = usersToNotify
            .DistinctBy(u => u.Id)
            .ToList();

            var clock = centralClockProvider.Now();

            var subjectValue = await propertyEvaluator.EvaluateAsync(mail.Subject.Operand, args);
            var textValue = await propertyEvaluator.EvaluateAsync(mail.Text.Operand, args);

            if (!(subjectValue is StateManagementTextValue subjectTextValue)) throw new InvalidOperationException("Mail subject operand must be of type text value");
            if (!(textValue is StateManagementTextValue textTextValue)) throw new InvalidOperationException("Mail text operand must be of type text value");

            foreach (var userToNotify in usersToNotify)
            {
                //TODO Notification comletion
                //var notification =
                //    new Notification(userToNotify, clock, textTextValue.TextValue, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, subject: subjectTextValue.TextValue);

                //notificationSender.SendNotifications(
                //      userToNotify.SectionDesignation,
                //      new List<Notification> { notification },
                //      notifySuccessLogger: (n, m) => { },
                //      notifyFailedLogger: (m, e) =>
                //      {
                //          traceLogger.Write(Setup.Tracing.TraceCategories.StateManagement, $"Failed sent task completion message : {m.Subject} to : {m.User?.Email} : {e}");
                //      });
            }

        }
    }
}
