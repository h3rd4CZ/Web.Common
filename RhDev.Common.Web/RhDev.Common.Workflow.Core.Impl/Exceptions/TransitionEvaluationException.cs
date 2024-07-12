using System;

namespace RhDev.Common.Workflow.Impl.Exceptions
{
    public class TransitionEvaluationException : Exception
    {
        private readonly Exception _exc;

        private readonly string _handler;
        public TransitionEvaluationException(Exception exc, string handler) : base(exc.Message, exc.InnerException)
        {
            _exc = exc;
            _handler = handler;
        }

        public override string ToString()
        {
            return $"HANDLER : {_handler} INNER : {InnerException?.ToString() ?? string.Empty}, EXC : {_exc}";
        }
    }
}
