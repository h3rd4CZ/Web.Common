using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using System.Collections.Generic;

namespace RhDev.Common.Workflow.Builder.Parts
{
    public class WorkflowTriggerBuilder : WorkflowPartBuilderBase<WorkflowTriggerBuilder, StateManagementTrigger>
    {
        public static WorkflowTriggerBuilder New(string name, int order)
        {
            var builder = new WorkflowTriggerBuilder() { };

            builder.Init();
            builder.product.Name = name;
            builder.product.Order = order;

            return builder;
        }
                
        public WorkflowTriggerBuilder AddHistory(WorkflowOperandBuilder entryBuilder, WorkflowOperandBuilder messageBuilder, WorkflowConditionExpressionBuilder? conditionBuilder = default, bool? add = true)
        {
            if (add is null || !add.Value) return this;

            var newHistory = new TransitionHistory();

            if(!Equals(null, entryBuilder)) newHistory.Entry.Operand = entryBuilder.Build();
            if(!Equals(null, messageBuilder)) newHistory.Message.Operand = messageBuilder.Build();

            if (conditionBuilder is not null) newHistory.Condition = conditionBuilder.Build();
            
            product.History.Add(newHistory);

            return this;
        }
                
        public WorkflowTriggerBuilder AddParameters(List<TransitionParameter> transitionParameters)
        {
            if (transitionParameters is null) return this;
            product.Parameters.AddRange(transitionParameters);
            return this;
        }

        public WorkflowTriggerBuilder AddParameter(WorkflowTransitionParameterBuilder builder)
        {
            product.Parameters.Add(builder.Build());
            return this;
        }
    }
}
