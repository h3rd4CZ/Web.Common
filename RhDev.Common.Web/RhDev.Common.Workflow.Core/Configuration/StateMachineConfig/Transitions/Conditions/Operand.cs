using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Builder;
using System.Collections.Generic;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions
{
    public class Operand  : IWorkflowPartBuilder
    {
        public WorkflowPropertyType Type { get; set; }
        public WorkflowDataType DataType { get; set; }
        public ArithmeticOperator Operator { get; set; }
        public List<Operand> Operands { get; set; } = new List<Operand>();
        public string Format { get; set; }
        public string Fetcher { get; set; }
        public string Text { get; set; }
        public string ArrayItemType { get; set; }
    }

}
