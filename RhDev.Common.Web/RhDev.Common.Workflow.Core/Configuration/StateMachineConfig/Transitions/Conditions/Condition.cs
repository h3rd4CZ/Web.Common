using System;
using System.Collections.Generic;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions
{
    [Serializable]
    public class Condition
    {
        public List<Operand> Operands { get; set; }
        public ComparativeOperator Operator { get; set; }
    }
}
