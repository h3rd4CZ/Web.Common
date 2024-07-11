using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Actions;

namespace RhDev.Common.Workflow.Builder.Parts
{
    public class WorkflowCustomActionParameterBuilder : WorkflowPartBuilderBase<WorkflowCustomActionParameterBuilder, StateMachineActionParameter>
    {
        public static WorkflowCustomActionParameterBuilder New(string name, WorkflowOperandBuilder operandBuilder)
        {
            var builder = new WorkflowCustomActionParameterBuilder();
            builder.Init();
            builder.product.Name = name;
            builder.product.Operand = operandBuilder.Build();
            return builder;
        }
    }
}
