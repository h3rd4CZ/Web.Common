using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using System;

namespace RhDev.Common.Workflow.Builder.Parts
{
    public class WorkflowConditionExpressionBuilder : WorkflowPartBuilderBase<WorkflowConditionExpressionBuilder, ConditionExpression>
    {
        public static WorkflowConditionExpressionBuilder New(ComparativeOperator comparativeOperator)
        {
            var builder = new WorkflowConditionExpressionBuilder() { };

            builder.Init();
            builder.product.Operator = comparativeOperator;

            return builder;
        }

        public WorkflowConditionExpressionBuilder AddAnd(WorkflowConditionExpressionBuilder conditionBuilder)
        {
            product.And.Add(conditionBuilder.Build());
            return this;
        }

        public WorkflowConditionExpressionBuilder AddOr(WorkflowConditionExpressionBuilder conditionBuilder)
        {
            product.Or.Add(conditionBuilder.Build());
            return this;
        }

        public WorkflowConditionExpressionBuilder SetNeg(WorkflowConditionExpressionBuilder conditionBuilder)
        {
            product.Neg = conditionBuilder.Build();
            return this;
        }

        public WorkflowConditionExpressionBuilder AddOperand(WorkflowOperandBuilder operandBuilder)
        {
            product.Operands.Add(operandBuilder.Build());
            return this;
        }
    }
}
