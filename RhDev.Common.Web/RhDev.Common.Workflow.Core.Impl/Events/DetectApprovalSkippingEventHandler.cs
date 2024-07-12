using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.Security;
using System.Collections.Generic;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;

namespace RhDev.Common.Workflow.Impl.Events
{
    public class DetectedApprovalSkippingEventHandler : ActionEventHandlerBase, IStateTransitionEventHandler
    {
        private readonly IStateManagementProcessor<StateManagementCommonTriggerProperties> _processor;

        public DetectedApprovalSkippingEventHandler(
            IPropertyEvaluator propertyEvaluator,
            IStateManagementProcessor<StateManagementCommonTriggerProperties> processor,
            IWorkflowMembershipProvider workflowMembershipProvider) : base(propertyEvaluator, workflowMembershipProvider)
        {
            _processor = processor;
            HandlerState = new Dictionary<string, string>();
        }
        public int EvaluationOrder => 5;

        public string UserData { get; }
        public IDictionary<string, string> HandlerState { get; }

        public async System.Threading.Tasks.Task RollbackAsync(
            object sender,
            SectionDesignation designation,
            StateManagementCommonTriggerProperties props,
            WorkflowDocumentIdentifier workflowDocumentIdentifier,
            Configuration.StateMachineConfig.Transitions.Transition transition,
            WorkflowTransitionLog transaction)
        {

        }

        public async System.Threading.Tasks.Task OnTransitionAsync(object sender, StateTransitionEventArgs args)
        {
            //TODO remove this handler
            //var parameters = StateMachineRuntimeParameters.Create(args.Metadata, args.WorkflowDocumentIdentifier, args.Parameters, default, default, true, false, true, null, WorkflowTransitionType.Unknown);

            //var permittedTriggers =
            //    _processor.GetAllPermittedTransition(parameters);

            //if (!permittedTriggers.Any()) return;

            //var caller = sender as ConfigurableStateMachineBase;

            //if(Equals(null, caller)) throw new InvalidOperationException("Caller is not ConfigurableStateMachineBase");

            //caller.SetApprovalSkippingDetected();
        }

        public bool HandleSpecificKind(object sender, StateTransitionEventArgs args)
        {
            return false;
        }
    }
}
