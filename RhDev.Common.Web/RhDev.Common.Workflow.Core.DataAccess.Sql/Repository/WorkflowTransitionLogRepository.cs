using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.DataAccess.Caching;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.DataAccess.Sql.Repository
{
    public class WorkflowTransitionLogRepository : DataStoreEntityRepositoryBase<WorkflowTransitionLog, DbContext>, IWorkflowTransitionLogRepository
    {
        public WorkflowTransitionLogRepository(
            RepositoryCacheSettings repositoryCacheSettings,
            IRepositoryCacheService<WorkflowTransitionLog> repositoryCacheService,
            IDatabaseAccessRepositoryFactory<DbContext> storeFactory) : base(repositoryCacheSettings, repositoryCacheService, storeFactory) { }

        public Task<IList<WorkflowTransitionLog>> GetAllForTransitionId(string transitionId)
        {
            Guard.StringNotNullOrWhiteSpace(transitionId);

            return ReadAsync(l => l.TransitionId == transitionId);
        }
    }
}
