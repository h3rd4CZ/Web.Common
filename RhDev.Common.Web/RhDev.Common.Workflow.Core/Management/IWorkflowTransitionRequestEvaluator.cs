using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Workflow.Entities;

namespace RhDev.Common.Workflow.Core.Management
{
    public interface IWorkflowTransitionRequestEvaluator : IAutoregisteredService
    {
        Task EvaluateTransitionAsync(WorkflowTransitionRequest request, bool async);
        Task RollbackTransitionAsync(int transitionRequestEntityId);
    }
}