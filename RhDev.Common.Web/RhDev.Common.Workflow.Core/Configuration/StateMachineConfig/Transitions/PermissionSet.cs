using RhDev.Common.Workflow.Builder;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions
{
    public class PermissionSet : IWorkflowPartBuilder
    {
        public Operand Operand { get; set; }
        public string Level { get; set; }
    }
}
