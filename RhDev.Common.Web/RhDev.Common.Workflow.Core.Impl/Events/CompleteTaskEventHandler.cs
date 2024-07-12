using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.Security;
using System;
using System.Collections.Generic;

namespace RhDev.Common.Workflow.Impl.Events
{
    public class CompleteTaskEventHandler : ActionEventHandlerBase, IStateTransitionEventHandler
    {
        private const string STATE_KEY_TASKID = "TaskId";

        public CompleteTaskEventHandler(
            IPropertyEvaluator propertyEvaluator,
            IWorkflowMembershipProvider workflowMembershipProvider
            ) : base(propertyEvaluator, workflowMembershipProvider)
        {
            HandlerState = new Dictionary<string, string>();
        }

        public int EvaluationOrder => 1;

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
            //var @params = DeserializeParams(transaction.Data);

            //var taskId = GetParam(@params, STATE_KEY_TASKID, false);
            //var nonCompletedTaskIds = GetParam(@params, STATE_KEY_NONCOMPLETEDTASKIDS, false);

            //var tasks = dataStoreAcessRepositoryFactory.GetStoreRepository<Task>();

            //if (!string.IsNullOrEmpty(nonCompletedTaskIds))
            //{
            //    string[] ids = nonCompletedTaskIds.Split(',');

            //    ids.Select(i => Convert.ToInt32(i)).ToList().ForEach(l =>
            //        {
            //            var task = tasks.ReadById(l);
            //            task.Status = DatabaseTaskStatus.NotStarted;
            //            task.ResolvedBy = 0;
            //            tasks.Update(task);
            //        }
            //    );

            //}

            //if (!string.IsNullOrEmpty(taskId))
            //{
            //    var tid = Convert.ToInt32(taskId);

            //    var docTask = tasks.ReadById(tid);
            //    docTask.ResolvedBy = 0;
            //    docTask.ResolvedOn = null;
            //    docTask.UserData = null;
            //    docTask.Status = DatabaseTaskStatus.NotStarted;
            //    tasks.Update(docTask);
            //}
        }

        public async Task OnTransitionAsync(object sender, StateTransitionEventArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            if (args.Transition.IsSystem) return;

            var stateMachine = sender as ConfigurableStateMachineBase;

            if (Equals(null, stateMachine))
                throw new InvalidOperationException($"Sender is not a ConfigurableStateMachineBase");
        }
    }
}
