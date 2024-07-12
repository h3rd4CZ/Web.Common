using Microsoft.Extensions.Logging;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.StateDefinition;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.Extensions;
using RhDev.Common.Workflow.PropertyModel.Properties;
using RhDev.Common.Workflow.Security;
using RhDev.Common.Web.Core;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Workflow.Impl.Events
{
    public class AssignNewTaskEventHandler : ActionEventHandlerBase, IStateTransitionEventHandler
    {
        private const string StateKeyTaskIds = "TaskIds";
        private const string StateKeyCurrentTaskGroupId = "CurrentTaskGroupId";

        private const string NOTIFICATION_SUBJECT_DEFAULT = "Seyfor workflow";
        private const string ProcessDocument = "Zpracovat dokument";

        public int EvaluationOrder => 6;

        private readonly IWorkflowTaskRepository workflowTaskRepository;
        private readonly IWorkflowInstanceRepository workflowInstanceRepository;
        private readonly ICentralClockProvider centralClockProvider;
        private readonly ILogger<AssignNewTaskEventHandler> traceLogger;

        public AssignNewTaskEventHandler(
            IWorkflowTaskRepository workflowTaskRepository,
            IWorkflowInstanceRepository workflowInstanceRepository,
            IPropertyEvaluator propertyEvaluator,
            IWorkflowMembershipProvider workflowMembershipProvider,
            ICentralClockProvider centralClockProvider,
            ILogger<AssignNewTaskEventHandler> traceLogger) : base(propertyEvaluator, workflowMembershipProvider)
        {
            HandlerState = new Dictionary<string, string>();
            this.workflowTaskRepository = workflowTaskRepository;
            this.workflowInstanceRepository = workflowInstanceRepository;
            this.centralClockProvider = centralClockProvider;
            this.traceLogger = traceLogger;
        }

        public string UserData { get; }
        public IDictionary<string, string> HandlerState { get; }

        public async Task RollbackAsync(
            object sender,
            SectionDesignation designation,
            StateManagementCommonTriggerProperties props,
            WorkflowDocumentIdentifier workflowDocumentIdentifier,
            Configuration.StateMachineConfig.Transitions.Transition transition,
            WorkflowTransitionLog transaction)
        {
            var currentTaskGroupId = GetParam<string>(StateKeyCurrentTaskGroupId, out bool taskGroupIdExist);
            if(taskGroupIdExist)
            {
                var instanceTemp = transaction.WorkflowInstance;
                var instance = await workflowInstanceRepository.ReadByIdAsync(instanceTemp.Id);

                instance.CurrentTaskGroupId = currentTaskGroupId == default ? default : new Guid(currentTaskGroupId);
                await workflowInstanceRepository.UpdateAsync(instance);
            }

            var taskIds = GetParam<List<int>>(StateKeyTaskIds, out bool taskExists);
            if(taskExists && taskIds?.Count > 0)
            {
                foreach (var taskToDel in taskIds)
                {
                    var task = await workflowTaskRepository.ReadAsync(t => t.Id == taskToDel);
                    if(task.Any()) await workflowTaskRepository.DeleteAsync(taskToDel);
                }
            }
        }

        public async Task OnTransitionAsync(object sender, StateTransitionEventArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            var transition = args.Transition;

            if (Equals(null, transition.Task)) return;

            args.CheckValidWorkflowId();

            var workflowTask = transition.Task;
            var taskRespondeType = workflowTask.TaskRespondeType == WorkflowTaskRespondType.Unknown ? WorkflowTaskRespondType.FirstWin: workflowTask.TaskRespondeType;
            var skipPermission = Equals(null, transition.Task.Permission) || transition.Task.Permission.Count == 0;
            var sendMail = !Equals(null, transition.Task.Mail?.Text?.Operand);
                        
            var stateMachine = sender as ConfigurableStateMachineBase;

            if (Equals(null, stateMachine))
                throw new InvalidOperationException($"Sender is not a ConfigurableStateMachineBase");

            var stateToDef = stateMachine.GetStateDefinitionByTitle(args.DestinationState);
            var stateFromDef = stateMachine.GetStateDefinitionByTitle(args.SourceState);          
            
            var membership = workflowTask.Assignee;

            Guard.NotNull(membership, nameof(membership));

            var extractGroups = workflowTask.GroupExtract;
            var assignedDate = DateTime.Now;
            DateTime? dueDate = stateToDef.DueDate > 0 ? assignedDate.AddBusinessDays(stateToDef.DueDate) : null;
            var taskTitle = GetTaskTitleFormated(stateToDef);

            var allAssignees = new List<(StateManagementUserValue userValue, List<UserInfo> extractedUsers)> { };

            foreach (var member in membership)
            {
                var memberAssignees = await GetAssigneesFor(member, extractGroups, args, sendMail);
                if (!Equals(null, memberAssignees)) allAssignees.AddRange(memberAssignees);
            }

            var allDistinctedAssignees = allAssignees
                .Where(a => !Equals(null, a.userValue) && !string.IsNullOrWhiteSpace(a.userValue.Name))
                .GroupBy(a => a.userValue.Name).Select(g => g.First()).ToList();

            var allOpenedTasks = await workflowTaskRepository.ReadAsync(t => t.WorkflowInstanceId == args.WorkflowId && t.Status == WorkflowDatabaseTaskStatus.NotStarted);
            foreach (var openedTask in allOpenedTasks) await workflowTaskRepository.DeleteAsync(openedTask.Id);

            string mailText = string.Empty;
            string mailSubject = string.Empty;

            if (sendMail)
            {
                var mailTextValue = await _propertyEvaluator.EvaluateAsync(workflowTask.Mail.Text.Operand, args);
                mailText = mailTextValue.ToString();

                var subject = workflowTask?.Mail?.Subject?.Operand;

                mailSubject = Equals(null, subject) ? NOTIFICATION_SUBJECT_DEFAULT : (await _propertyEvaluator.EvaluateAsync(subject, args))?.ToString();

                mailSubject = mailSubject ?? NOTIFICATION_SUBJECT_DEFAULT;
            }

            var groupId = Guid.NewGuid();

            if (allDistinctedAssignees.Count == 0) throw new InvalidOperationException("There are no assignees to assign this task");

            var taskIds = new List<int>();
            foreach (var assignee in allDistinctedAssignees)
            {
                var task = new WorkflowTask
                {
                    AssignedOn = assignedDate,
                    AssignedTo = assignee.userValue.Id,
                    GroupId = groupId,
                    AssigneeType =
                        assignee.userValue.IsPermissionGroup
                        ? TaskAssigneeType.PermissionGroup
                        : TaskAssigneeType.User,
                    DueDate = dueDate,
                    Status = WorkflowDatabaseTaskStatus.NotStarted,
                    Title = taskTitle,
                    WorkflowInstanceId = args.WorkflowId,
                    TaskRespondeType = taskRespondeType
                };

                await workflowTaskRepository.CreateAsync(task);
                taskIds.Add(task.Id);
            }

            SetStateParam(StateKeyTaskIds, taskIds);

            var wfInstance = await workflowInstanceRepository.ReadByIdAsync(args.WorkflowId);

            var groupIdBefore = wfInstance.CurrentTaskGroupId?.ToString();
            wfInstance.CurrentTaskGroupId = groupId;
            await workflowInstanceRepository.UpdateAsync(wfInstance);

            SetStateParam(StateKeyCurrentTaskGroupId, groupIdBefore);
                        
            if (sendMail)
            {
                SendNotificationFor(allDistinctedAssignees, args, mailText, mailSubject);
            }
        }

        private void SendNotificationFor(List<(StateManagementUserValue userValue, List<UserInfo> extractedUsers)> assignees, StateTransitionEventArgs args, string text, string subject)
        {
            Guard.NotNull(assignees, nameof(assignees));
            Guard.NotNull(args, nameof(args));

            var clock = centralClockProvider.Now();

            var allRecipients = new List<UserInfo> { };

            foreach (var assignee in assignees)
            {
                var allRecipientsInThisGroup = assignee.userValue.IsPermissionGroup
                ? assignee.extractedUsers?.ToList()
                : new List<UserInfo> { BuildUserInfo(assignee.userValue) };

                allRecipientsInThisGroup = allRecipientsInThisGroup.GroupBy(a => a.Id).Select(a => a.First()).ToList();

                allRecipients.AddRange(allRecipientsInThisGroup);
            }

            allRecipients = allRecipients.GroupBy(a => a.Id).Select(a => a.First()).ToList();


            foreach (var recipient in allRecipients.Where(a => !Equals(null, a)))
            {
                //TODO Notification sender when assigning new Task
            }
        }

        private UserInfo BuildUserInfo(StateManagementUserValue userValue)
        {
            Guard.NotNull(userValue, nameof(userValue));

            if (userValue.IsPermissionGroup) throw new InvalidOperationException($"Cannot build user info because userValue is group");

            return new UserInfo(userValue.Section, userValue.Id, userValue.Name, userValue.DisplayName, userValue.Email);
        }

        private string GetTaskTitleFormated(StateDefinition stateToDef)
        {
            if (stateToDef == null) throw new ArgumentNullException(nameof(stateToDef));
            
            string taskTitleFormat = stateToDef.TaskTitleFormat;

            if (string.IsNullOrEmpty(taskTitleFormat)) return ProcessDocument;
                        
            return taskTitleFormat;

        }
    }
}
