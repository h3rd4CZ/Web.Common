using System.Xml.Serialization;

namespace RhDev.Common.Web.Core.Workflow
{
    public class WorkflowTransitionCustomProperty
    {
        [XmlElement("Key")]
        public string Key { get; set; }
        [XmlElement("Value")]
        public string Value { get; set; }
    }
}

