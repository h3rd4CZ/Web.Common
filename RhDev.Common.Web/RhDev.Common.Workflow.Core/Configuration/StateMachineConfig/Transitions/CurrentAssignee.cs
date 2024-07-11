using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions
{
    [Serializable]
    public class CurrentAssignee
    {
        public Operand Property { get; set; }
    }
}
