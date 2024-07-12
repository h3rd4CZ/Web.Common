using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.PropertyModel.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Impl.Evaluators
{
    public class ComputeOperandEvaluator : OperandEvaluationEvaluatorBase, IComputeOperandEvaluator
    {
        public async Task<StateManagementValue> EvaluateAsync(IPropertyEvaluator propertyEvaluator, Operand property, StateTransitionEventArgs args)
        {
            Guard.NotNull(propertyEvaluator, nameof(propertyEvaluator));
            Guard.NotNull(args, nameof(args));
            Guard.NotNull(property, nameof(property));

            var @operator = property.Operator;

            if (@operator == ArithmeticOperator.Unknown) throw new InvalidOperationException("Arithmetic operator is not defined");

            var operandsTask = property
                    .Operands?
                    .Select(async o => await propertyEvaluator.EvaluateAsync(o, args))
                    .ToList();

            var evaluatedOperands = operandsTask is null ? new List<StateManagementValue> { }.ToArray() : await Task.WhenAll(operandsTask);

            return Evaluate(@operator, evaluatedOperands);
        }

        private StateManagementValue Evaluate(ArithmeticOperator @operator, IList<StateManagementValue> operands)
        {
            switch (@operator)
            {
                case ArithmeticOperator.Add:
                    {
                        ValidateOperandCount(2, operands.Count);
                        return operands[0] + operands[1];
                    }
                case ArithmeticOperator.Sub:
                    {
                        ValidateOperandCount(2, operands.Count);
                        return operands[0] - operands[1];
                    }
                case ArithmeticOperator.Mul:
                    {
                        ValidateOperandCount(2, operands.Count);
                        return operands[0] * operands[1];
                    }
                case ArithmeticOperator.Div:
                    {
                        ValidateOperandCount(2, operands.Count);
                        return operands[0] / operands[1];
                    }
                case ArithmeticOperator.Substring:
                    {
                        ValidateMinOperandCount(2, operands.Count);
                        ValidateMaxOperandCount(3, operands.Count);

                        return operands[0].Substring(operands[1], operands.Count == 3 ? operands[2] : null);
                    }
                case ArithmeticOperator.AddYears: return EvaluateDateTimeComputation(operands, (d, n) => d.AddYears(n));

                case ArithmeticOperator.AddMonths: return EvaluateDateTimeComputation(operands, (d, n) => d.AddMonths(n));

                case ArithmeticOperator.AddDays: return EvaluateDateTimeComputation(operands, (d, n) => d.AddDays(n));

                case ArithmeticOperator.AddHours: return EvaluateDateTimeComputation(operands, (d, n) => d.AddHours(n));

                case ArithmeticOperator.AddMinutes: return EvaluateDateTimeComputation(operands, (d, n) => d.AddMinutes(n));

                case ArithmeticOperator.Push:
                    {
                        ValidateOperandCount(2, operands.Count);
                        ValidateNotNullOperands(operands);

                        return operands[0].Push(operands[1]);
                    }
                case ArithmeticOperator.Length:
                    {
                        ValidateOperandCount(1, operands.Count);
                        ValidateNotNullOperands(operands);

                        return operands[0].Length();
                    }
                case ArithmeticOperator.Clear:
                    {
                        ValidateOperandCount(1, operands.Count);
                        ValidateNotNullOperands(operands);

                        return operands[0].Clear();
                    }
                case ArithmeticOperator.Neg:
                    {
                        ValidateOperandCount(1, operands.Count);

                        return !operands[0];
                    }
                case ArithmeticOperator.Format:
                    {
                        ValidateMinOperandCount(1, operands.Count);
                        var text = operands[0];

                        return text.Format(operands.Skip(1).ToList());
                    }
                case ArithmeticOperator.Pop:
                    {
                        ValidateOperandCount(1, operands.Count);
                        ValidateNotNullOperands(operands);

                        return operands[0].Pop();
                    }
                case ArithmeticOperator.Index:
                    {
                        ValidateOperandCount(2, operands.Count);
                        ValidateNotNullOperands(operands);

                        return operands[0].Index(operands[1]);
                    }
                case ArithmeticOperator.Abs:
                    {
                        ValidateOperandCount(1, operands.Count);
                        ValidateNotNullOperands(operands);

                        return operands[0].Abs();
                    }
                default: throw new NotSupportedException($"{@operator} is not supported arithmetic operator");
            }
        }

        private StateManagementValue EvaluateDateTimeComputation(IList<StateManagementValue> operands, Func<StateManagementValue, StateManagementValue, StateManagementValue> factory)
        {
            ValidateOperandCount(2, operands.Count);
            ValidateNotNullOperands(operands);

            return factory(operands[0], operands[1]);
        }
    }
}
