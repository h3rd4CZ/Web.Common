using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;
using System.Text.Json.Serialization;

namespace RhDev.Common.Web.Core.DataAccess.Workflow
{
    [Serializable]
    public class WorkflowTransitionRequestPayload
    {
        public StateMachineRuntimeParameters RuntimeParameters { get; init; }

        public string Source { get; set; } = "Common";
        public string Title { get; set; }

        [JsonIgnore]
        public bool EvaluateSystemTriggers => RuntimeParameters.EvaluateSystemTriggers;
        [JsonIgnore]
        public bool EvaluateMembershipLocally => RuntimeParameters.EvaluateMembershipLocally;
        [JsonIgnore]
        public WorkflowDocumentIdentifier DocumentIdentifier => RuntimeParameters.DocumentMetadataIdentifier;
        [JsonIgnore]
        public StateManagementCommonTriggerProperties TransitionProperties => RuntimeParameters.UserData;
        [JsonIgnore]
        public List<string> UserGroups => RuntimeParameters.UserPermissionGroups;
        [JsonIgnore]
        public WorkflowInfo Workflow => RuntimeParameters.Workflow;
        [JsonIgnore]
        public TransitionRequestType TransitionType => RuntimeParameters.TransitionType;
        public WorkflowTransitionRequestRollbackInfo RollbackInfo { get; set; }

        public static WorkflowTransitionRequestPayload Create(StateMachineRuntimeParameters runtimeParameters, string source, string requestTitle = default!)
        {
            Guard.NotNull(runtimeParameters);

            return new WorkflowTransitionRequestPayload
            {
                RuntimeParameters = runtimeParameters,
                Source = source,
                Title = requestTitle,
            };
        }

        public override string ToString()
        {
            return $"Přechod dokumentu {Title} {DocumentIdentifier}";
        }
    }
}
