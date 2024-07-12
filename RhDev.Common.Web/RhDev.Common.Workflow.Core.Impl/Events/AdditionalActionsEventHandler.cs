using RhDev.Common.Workflow.Actions;
using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;

namespace RhDev.Common.Workflow.Impl.Events
{
    public class AdditionalActionsEventHandler : ActionEventHandlerBase, IStateTransitionEventHandler
    {
        private readonly IStateManagementConfiguredActionManager _actionManager;

        public int EvaluationOrder => 4;

        public AdditionalActionsEventHandler(
            IPropertyEvaluator propertyEvaluator,
            IWorkflowMembershipProvider workflowMembershipProvider,
            IStateManagementConfiguredActionManager actionManager) : base(propertyEvaluator, workflowMembershipProvider)
        {
            _actionManager = actionManager;
        }
               
        
        public async Task RollbackAsync(
            object sender,
            SectionDesignation designation,
            StateManagementCommonTriggerProperties props,
            WorkflowDocumentIdentifier workflowDocumentIdentifier,
            Configuration.StateMachineConfig.Transitions.Transition transition,
            WorkflowTransitionLog transaction)
        {
            if (props == null) throw new ArgumentNullException(nameof(props));
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            //var parameters = DeserializeParams(transaction.Data);

            //if (Equals(null, parameters)) return;

            //Guard.NotNull(transition.AdditionalActions, nameof(transition.AdditionalActions));

            //await _actionManager.RollbackAsync(parameters, designation, transition.AdditionalActions);
        }

        public async Task OnTransitionAsync(object sender, StateTransitionEventArgs args)
        {
            var actions = args.Transition.AdditionalActions;

            if (Equals(null, actions) || actions.Count == 0) return;

            try
            {
                await _actionManager.RunAsync(args, actions);
            }
            finally
            {
                foreach (var stateManagementCompletedAction in _actionManager.CompletedActions)
                    HandlerState[stateManagementCompletedAction.Identifier] = stateManagementCompletedAction.Params;
            }
        }

        public bool HandleSpecificKind(object sender, StateTransitionEventArgs args)
        {
            return false;
        }
    }
}
