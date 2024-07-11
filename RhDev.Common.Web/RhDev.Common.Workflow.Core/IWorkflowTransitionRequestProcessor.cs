using RhDev.Common.Web.Core.Composition;
using System;
using System.Collections.Generic;
using System.Text;

namespace RhDev.Common.Workflow
{
    public interface IWorkflowTransitionRequestProcessor : IAutoregisteredService
    {
        void ProcessRequest(int requestId, bool throwExceptionOnFail);
        void RollbackRequest(int requestId, bool writeFail);
    }
}
