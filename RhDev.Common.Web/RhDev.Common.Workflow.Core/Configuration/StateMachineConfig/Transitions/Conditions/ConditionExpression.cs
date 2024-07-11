using RhDev.Common.Workflow.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions
{
    public class ConditionExpression : IWorkflowPartBuilder
    {
        public List<ConditionExpression> And { get; set; } = new List<ConditionExpression>();
        public List<ConditionExpression> Or { get; set; } = new List<ConditionExpression>();
        public ConditionExpression Neg { get; set; }
        public ComparativeOperator Operator { get; set; }
        public List<Operand> Operands { get; set; } = new List<Operand>();
    }
}
