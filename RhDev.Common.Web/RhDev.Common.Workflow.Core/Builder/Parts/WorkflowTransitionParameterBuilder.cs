using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;

namespace RhDev.Common.Workflow.Builder.Parts
{
    public class WorkflowTransitionParameterBuilder : WorkflowPartBuilderBase<WorkflowTransitionParameterBuilder, TransitionParameter>
    {
        public static WorkflowTransitionParameterBuilder New(WorkflowDataType type, string propertyName, string displayName, bool required)
        {
            var builder = new WorkflowTransitionParameterBuilder();
            builder.Init();
            builder.product.Type = type;
            builder.product.PropertyName = propertyName;
            builder.product.DisplayName = displayName;
            builder.product.Required= required;
            return builder;
        }
    }
}
