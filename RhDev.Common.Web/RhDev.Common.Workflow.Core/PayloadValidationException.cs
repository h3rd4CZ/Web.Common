using System;

namespace RhDev.Common.Workflow
{
    public class PayloadValidationException : Exception
    {
        public PayloadValidationException(string msg) : base(msg) { }
    }
}
