using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.DataAccess.Workflow;

namespace RhDev.Common.Workflow.Core.Management
{
    public interface IExternalWorkflowHistoryProvider : IAutoregisteredService
    {
        Task WriteAsync(WorkflowDocumentIdentifier workflowDocumentIdentifier, DateTime date, string? @event, string? message, string user);
    }
}
