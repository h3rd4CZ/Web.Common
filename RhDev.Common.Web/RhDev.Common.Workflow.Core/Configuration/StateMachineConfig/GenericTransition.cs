using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig
{
    [Serializable]
    public class GenericTransition
    {
        [XmlArray("ForStates")]
        [XmlArrayItem("Code")]
        public List<string> ForStates { get; set; }

        [XmlElement("Transition")]
        public Transition Transition { get; set; }
    }
}
