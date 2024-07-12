﻿using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.DataAccess.Caching;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using Microsoft.EntityFrameworkCore;

namespace RhDev.Common.Workflow.DataAccess.Sql.Repository
{
    public class WorkflowInstanceSystemProcessingInfoRepository : DataStoreEntityRepositoryBase<WorkflowInstanceSystemProcessingInfo, DbContext>, IWorkflowInstanceSystemProcessingInfoRepository

    {
        public WorkflowInstanceSystemProcessingInfoRepository(
            RepositoryCacheSettings repositoryCacheSettings,
            IRepositoryCacheService<WorkflowInstanceSystemProcessingInfo> repositoryCacheService,
            IDatabaseAccessRepositoryFactory<DbContext> storeFactory) : base(repositoryCacheSettings, repositoryCacheService, storeFactory) { }

    }
}
