using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using RhDev.Common.Workflow.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;

namespace RhDev.Common.Workflow.Events
{
    public class StateTransitionEventArgs : EventArgs
    {
        public SectionDesignation Designation;
        public readonly StateManagementCommonTriggerProperties Parameters;
        public readonly string SourceState;
        public readonly string DestinationState;
        public readonly Transition Transition;
        public string UserId { get; set; }
        public IList<string> UserGroups { get; set; }
        public bool EvaluateMembershipLocally { get; set; }
        public int WorkflowId { get; set; }

        public void CheckValidWorkflowId()
        {
            if (WorkflowId == default) throw new InvalidOperationException("Workflow id is not valid");
        }

        public WorkflowDocumentIdentifier WorkflowDocumentIdentifier { get; set; }

        public void SetDesignation(SectionDesignation d) => Designation = d;
                
        public StateTransitionEventArgs
            (SectionDesignation designation,
            StateManagementCommonTriggerProperties task,
            string sourceState,
            string destinationState,
            Transition transition,
            WorkflowDocumentIdentifier workflowDocumentIdentifier,
            int workflowId,
            string userId,
            IList<string> userGroups,
            bool evaluateMembershipLocally)
        {
            Designation = designation;
            Parameters = task;
            SourceState = sourceState;
            DestinationState = destinationState;
            Transition = transition;
            WorkflowDocumentIdentifier = workflowDocumentIdentifier;
            WorkflowId = workflowId;
            UserId = userId;
            UserGroups = userGroups;
            EvaluateMembershipLocally = evaluateMembershipLocally;
        }
    }
}
