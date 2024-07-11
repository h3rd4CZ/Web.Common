using RhDev.Common.Web.Core.Composition;
using System;

namespace RhDev.Common.Workflow
{
    public interface IWorkflowTransitionRequestProcessorFactory : IAutoregisteredService
    {
        IWorkflowTransitionRequestProcessor GetRequestProcessor(Guid listId);
    }
}
