using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Builder;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Actions;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions
{
    [Serializable]
    public class Transition : IWorkflowPartBuilder
    {
        public bool IsSystem { get; set; }

        public bool WithoutPermission { get; set; }

        public string State { get; set; }

        public ConditionExpression Condition { get; set; }

        public StateManagementTrigger StateManagementTrigger { get; set; }

        public List<StateMachineConfiguredAction> AdditionalActions { get; set; } = new List<StateMachineConfiguredAction>();

        public WorkflowAssigneeTask Task { get; set; }

        [JsonIgnore]
        public bool IsReentrant { get; set; }

        public string SaveTaskGroupId { get; set; }

        public List<WorkflowTransitionCustomProperty> CustomProperties { get; set; } = new List<WorkflowTransitionCustomProperty>();
    }
}
