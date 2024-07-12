using System;
using System.Linq;

namespace RhDev.Common.Workflow.Impl.Evaluators
{
    public abstract class OperandEvaluationEvaluatorBase
    {
        protected void ValidateMinOperandCount(int expected, int actual)
        {
            if (expected > actual) throw new InvalidOperationException($"Operation must have at least {expected} operands but found : {actual}");
        }

        protected void ValidateMaxOperandCount(int expected, int actual)
        {
            if (expected < actual) throw new InvalidOperationException($"Operation must have max {expected} operands but found : {actual}");
        }

        protected void ValidateOperandCount(int expected, int actual)
        {
            if (expected != actual) throw new InvalidOperationException($"Operation must have {expected} operands but found : {actual}");
        }

        protected void ValidateNotNullOperands(params object[] operands)
        {
            if (operands.Any(o => Equals(null, o))) throw new InvalidOperationException("At least one operand is null");
        }
    }
}
