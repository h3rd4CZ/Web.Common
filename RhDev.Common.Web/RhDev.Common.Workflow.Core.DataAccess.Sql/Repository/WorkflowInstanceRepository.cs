using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.DataAccess.Caching;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace RhDev.Common.Workflow.DataAccess.Sql.Repository
{
    public class WorkflowInstanceRepository : DataStoreEntityRepositoryBase<WorkflowInstance, DbContext>, IWorkflowInstanceRepository
    {
        public WorkflowInstanceRepository(
            RepositoryCacheSettings repositoryCacheSettings,
            IRepositoryCacheService<WorkflowInstance> repositoryCacheService,
            IDatabaseAccessRepositoryFactory<DbContext> storeFactory) : base(repositoryCacheSettings, repositoryCacheService, storeFactory) { }

    }
}
