using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Actions;

namespace RhDev.Common.Workflow.Builder.Parts
{
    public class WorkflowCustomActionBuilder : WorkflowPartBuilderBase<WorkflowCustomActionBuilder, StateMachineConfiguredAction>
    {
        public static WorkflowCustomActionBuilder New(string typeName)
        {
            var builder = new WorkflowCustomActionBuilder();
            builder.Init();
            builder.product.TypeName = typeName;

            return builder;
        }

        public WorkflowCustomActionBuilder WithId(string id)
        {
            product.Id = id;
            return this;
        }

        public WorkflowCustomActionBuilder WithPreCondition(WorkflowConditionExpressionBuilder workflowConditionExpressionBuilder)
        {
            product.Condition = workflowConditionExpressionBuilder.Build();
            return this;
        }

        public WorkflowCustomActionBuilder SuppressException(bool suppress)
        {
            product.SuppressException = suppress;
            return this;
        }

        public WorkflowCustomActionBuilder Disabled(bool disabled)
        {
            product.Disabled = disabled;
            return this;
        }
                                
        public WorkflowCustomActionBuilder AddParameter(WorkflowCustomActionParameterBuilder parameterBuilder)
        {
            product.Parameters.Add(parameterBuilder.Build());

            return this;
        }
    }
}
