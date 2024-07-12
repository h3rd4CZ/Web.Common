﻿using RhDev.Common.Web.Core.DataAccess;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;

namespace RhDev.Common.Workflow.Core.DataAccess.Sql.Repository
{
    public interface IWorkflowTransitionLogRepository : IStoreRepository<WorkflowTransitionLog>
    {
        Task<IList<WorkflowTransitionLog>> GetAllForTransitionId(string transitionId);
    }
}
