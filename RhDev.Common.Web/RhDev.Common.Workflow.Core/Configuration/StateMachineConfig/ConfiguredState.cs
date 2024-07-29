using RhDev.Common.Workflow.Builder;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig
{
    [Serializable]
    public class ConfiguredState : IWorkflowPartBuilder
    {
        public string Code { get; set; }
        public WorkflowTaskMail CompletionMail { get; set; } = default!;

        public List<Transition> Transitions { get; set; } = new List<Transition>();

    }
}
