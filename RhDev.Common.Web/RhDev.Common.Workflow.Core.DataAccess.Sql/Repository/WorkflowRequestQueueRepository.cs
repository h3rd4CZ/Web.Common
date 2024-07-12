using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.DataAccess.Caching;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Workflow;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Workflow.Entities;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RhDev.Common.Workflow.DataAccess.Sql.Repository
{
    public class WorkflowTransitionRequestRepository : DataStoreEntityRepositoryBase<WorkflowTransitionRequest, DbContext>, IWorkflowTransitionRequestRepository
    {
        public WorkflowTransitionRequestRepository(
            RepositoryCacheSettings repositoryCacheSettings,
            IRepositoryCacheService<WorkflowTransitionRequest> repositoryCacheService,
            IDatabaseAccessRepositoryFactory<DbContext> storeFactory) : base(repositoryCacheSettings, repositoryCacheService, storeFactory) { }

        public async Task<IList<WorkflowTransitionRequest>> GetFailRequestsWithRepetationEqualAsync(int numOfRepetation) 
            => await ReadAsync(r => r.State == TransitionTaskStatus.Failed && r.RepeatCount == numOfRepetation);

        public async Task<IList<WorkflowTransitionRequest>> GetFailRequestsWithRepetationLessOrEqualThenAsync(int numOfRepetation) 
            => await ReadAsync(r => r.State == TransitionTaskStatus.Failed && r.RepeatCount <= numOfRepetation);

        public async Task<IList<WorkflowTransitionRequest>> GetRequestsOlderThenAsync(DateTime date)
        {
            Guard.NotDefault(date);

            return await ReadAsync(r => r.Created < date);
        }

        public async Task PushRequestAsync(WorkflowTransitionRequest entity) => await CreateAsync(entity);

        public async Task RemoveRequestAsync(WorkflowTransitionRequest entity) => await DeleteAsync(entity.Id);
    }
}
