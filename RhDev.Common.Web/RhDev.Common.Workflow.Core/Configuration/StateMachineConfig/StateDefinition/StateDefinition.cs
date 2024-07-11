using RhDev.Common.Workflow.Builder;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig.StateDefinition
{
    [Serializable]
    public class StateDefinition : IWorkflowPartBuilder
    {
        [XmlElement("Title")]
        public string Title { get; set; }

        [XmlElement("Code")]
        public string Code { get; set; }

        [XmlElement("IsEnd")]
        public bool IsEnd { get; set; }

        [XmlElement("IsStart")]
        public bool IsStart { get; set; }

        [XmlArray("Aliases")]
        [XmlArrayItem("Alias")]
        public List<string> Aliases { get; set; }

        [XmlElement("NotifyEmptyGroup")]
        public bool NotifyEmptyGroup { get; set; }

        [XmlElement("TaskTitleFormat")]
        public string TaskTitleFormat { get; set; }

        [XmlElement("DueDate")]
        public int DueDate { get; set; }
    }
}
