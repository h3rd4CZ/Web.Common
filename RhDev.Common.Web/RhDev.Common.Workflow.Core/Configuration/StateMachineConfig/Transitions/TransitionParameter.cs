using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Builder;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions
{
    public class TransitionParameter : IWorkflowPartBuilder
    {
        public string PropertyName { get; set; }
        public string DisplayName { get; set; }
        public WorkflowDataType Type { get; set; }
        public bool Required { get; set; }
    }
}
