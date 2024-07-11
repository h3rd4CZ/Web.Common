using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RhDev.Common.Workflow.Entities
{
    public class TriggerInfo
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public int Order { get; set; }
        public List<TransitionParameter> Parameters { get; set; }
    }
}
