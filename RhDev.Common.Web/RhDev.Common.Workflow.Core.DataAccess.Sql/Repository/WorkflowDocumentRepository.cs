using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.DataAccess.Caching;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;


namespace RhDev.Common.Workflow.DataAccess.Sql.Repository
{

    public class WorkflowDocumentRepository : DataStoreEntityRepositoryBase<WorkflowDocument, DbContext>, IWorkflowDocumentRepository
    {
        public WorkflowDocumentRepository(
            RepositoryCacheSettings repositoryCacheSettings,
            IRepositoryCacheService<WorkflowDocument> repositoryCacheService,
            IDatabaseAccessRepositoryFactory<DbContext> storeFactory) : base(repositoryCacheSettings, repositoryCacheService, storeFactory) { }
        
    }
}
