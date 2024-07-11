using System.Collections.Generic;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Web.Core.DataAccess.Workflow
{
    public class StateMachineRuntimeParameters
    {
        public StateManagementCommonTriggerProperties UserData { get; set; }
        public WorkflowDocumentIdentifier DocumentMetadataIdentifier { get; set; }
        public string UserId { get; set; }
        public List<string> UserPermissionGroups { get; set; }
        public bool EvaluateMembershipLocally { get; set; }
        public bool EvaluateSystemTriggers { get; set; }
        public WorkflowInfo Workflow { get; set; }
        public TransitionRequestType TransitionType { get; set; }

        public static StateMachineRuntimeParameters Create(
            WorkflowDocumentIdentifier workflowDocumentIdentifier,
            StateManagementCommonTriggerProperties userData,
            string userId, List<string> userGroups,
            bool evaluateSystemTriggers,
            bool evaluateMembershipLocally,
            WorkflowInfo workflow,
            TransitionRequestType transitionType) => new StateMachineRuntimeParameters
            {
                DocumentMetadataIdentifier = workflowDocumentIdentifier,
                UserData = userData,
                UserPermissionGroups = userGroups,
                EvaluateMembershipLocally = evaluateMembershipLocally,
                UserId = userId,
                EvaluateSystemTriggers = evaluateSystemTriggers,
                Workflow = workflow,
                TransitionType = transitionType
            };

        public static StateMachineRuntimeParameters Create(WorkflowTransitionRequestPayload payload, DocumentWorkflowMetadata workflowDocument)
        {
            Guard.NotNull(payload.TransitionProperties);

            return new StateMachineRuntimeParameters
            {
                UserData = payload.TransitionProperties,
                DocumentMetadataIdentifier = payload.DocumentIdentifier,
                EvaluateSystemTriggers = payload.EvaluateSystemTriggers,
                EvaluateMembershipLocally = payload.EvaluateMembershipLocally,
                UserPermissionGroups = payload.UserGroups,
                UserId = payload.TransitionProperties.UserRespondedId,
                Workflow = payload.Workflow,
                TransitionType = payload.TransitionType

            };
        }

    }
}
