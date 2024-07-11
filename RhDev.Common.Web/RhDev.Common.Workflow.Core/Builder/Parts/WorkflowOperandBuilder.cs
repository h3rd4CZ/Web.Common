using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RhDev.Common.Workflow.Builder.Parts
{
    public class WorkflowOperandBuilder : WorkflowPartBuilderBase<WorkflowOperandBuilder, Operand>
    {
        public static WorkflowOperandBuilder New(WorkflowDataType workflowDataType)
        {
            var builder = new WorkflowOperandBuilder() { };

            builder.Init();
            builder.product.DataType = workflowDataType;

            return builder;
        }
                
        public WorkflowOperandBuilder WithPropertyType(WorkflowPropertyType workflowPropertyType)
        {
            product.Type = workflowPropertyType;
            return this;
        }

        public WorkflowOperandBuilder WithOperator(ArithmeticOperator arithmeticOperator)
        {
            product.Operator = arithmeticOperator;
            return this;
       
        }

        public WorkflowOperandBuilder WithFormat(string format)
        {
            product.Format = format;
            return this;
        }

        public WorkflowOperandBuilder WithFetcher(string fetcher)
        {
            product.Fetcher = fetcher;
            return this;
        }

        public WorkflowOperandBuilder WithText(string text)
        {
            product.Text = text;
            return this;
        }

        public WorkflowOperandBuilder AddOperand(WorkflowOperandBuilder operandBuilder)
        {
            product.Operands.Add(operandBuilder.Build());
            return this;
        }

        public WorkflowOperandBuilder AddOperands(List<WorkflowOperandBuilder> operandsBuilder)
        {
            product.Operands.AddRange(operandsBuilder.Select(b => b.Build()).ToList());
            return this;
        }

        public static WorkflowOperandBuilder True => WorkflowOperandBuilder.New(WorkflowDataType.Boolean).WithText(true.ToString());
        public static WorkflowOperandBuilder False => WorkflowOperandBuilder.New(WorkflowDataType.Boolean).WithText(false.ToString());
        public static WorkflowOperandBuilder Text(string text) => WorkflowOperandBuilder.New(WorkflowDataType.Text).WithText(text);
    }
}
