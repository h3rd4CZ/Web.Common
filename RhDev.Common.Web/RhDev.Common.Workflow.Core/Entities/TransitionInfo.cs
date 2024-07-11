using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RhDev.Common.Workflow.Entities
{
    public class TransitionInfo
    {
        public TriggerInfo Trigger { get; set; }
        public List<WorkflowTransitionCustomProperty> CustomProperties { get; set; }
        public override string ToString() => $"Trigger code : {Trigger?.Code}, name : {Trigger?.Name}";
    }
}
