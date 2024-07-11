using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Core.Exceptions
{
    public class DocumentWorkflowInicializationException : Exception
    {
        public DocumentWorkflowInicializationException(string msg, Exception innerException) : base(msg, innerException) { }
        public DocumentWorkflowInicializationException() { }
    }
}
