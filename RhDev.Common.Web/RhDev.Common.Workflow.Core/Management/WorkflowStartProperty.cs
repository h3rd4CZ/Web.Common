using RhDev.Common.Web.Core.Workflow;

namespace RhDev.Common.Workflow.Management
{
    public class WorkflowStartProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string ForArray { get; set; }
        public WorkflowDataType Type { get; set; }
        public string ArrayItemType { get; set; }
    }
}
