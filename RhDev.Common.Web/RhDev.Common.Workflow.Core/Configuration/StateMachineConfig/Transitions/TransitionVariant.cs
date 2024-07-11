using System;
using System.Collections.Generic;
using System.Text;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions
{
    public class TransitionVariant
    {
        public string Code { get; set; }
        public string DisplayName { get; set; }
        public List<TransitionParameter> Parameters { get; set; }
    }
}
