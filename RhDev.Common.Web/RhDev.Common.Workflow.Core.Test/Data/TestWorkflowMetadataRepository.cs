using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.DataAccess.Caching;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Core.Test.Data
{
    public class TestWorkflowMetadataRepository : DataStoreEntityRepositoryBase<TestWorkflowMetadata, WorkflowDatabaseTestContext>, ITestWorkflowMetadataRepository
    {
        public TestWorkflowMetadataRepository(
            RepositoryCacheSettings repositoryCacheSettings,
            IRepositoryCacheService<TestWorkflowMetadata> repositoryCacheService,
            IDatabaseAccessRepositoryFactory<WorkflowDatabaseTestContext> storeFactory) : base(repositoryCacheSettings, repositoryCacheService, storeFactory) { }

    }
}
