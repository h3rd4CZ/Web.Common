using RhDev.Common.Web.Core.DataAccess;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;

namespace RhDev.Common.Workflow.Core.DataAccess.Sql.Repository
{
    public interface IWorkflowTransitionRequestRepository : IStoreRepository<WorkflowTransitionRequest> 
    {
        Task PushRequestAsync(WorkflowTransitionRequest entity);
        Task<IList<WorkflowTransitionRequest>> GetRequestsOlderThenAsync(DateTime date);
        Task RemoveRequestAsync(WorkflowTransitionRequest entity);
        Task<IList<WorkflowTransitionRequest>> GetFailRequestsWithRepetationLessOrEqualThenAsync(int numOfRepetation);
        Task<IList<WorkflowTransitionRequest>> GetFailRequestsWithRepetationEqualAsync(int numOfRepetation);
    }
}
