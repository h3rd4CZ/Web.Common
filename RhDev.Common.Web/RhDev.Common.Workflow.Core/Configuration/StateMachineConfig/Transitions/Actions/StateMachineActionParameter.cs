using RhDev.Common.Workflow.Builder;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Actions
{
    [Serializable]
    public class StateMachineActionParameter : IWorkflowPartBuilder
    {
        public string Name { get; set; }

        public Operand Operand { get; set; }
    }
}
