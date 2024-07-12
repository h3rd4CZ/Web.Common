using Microsoft.AspNetCore.Identity;
using RhDev.Common.Web.Core.DataAccess;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;

namespace RhDev.Common.Workflow.Core.DataAccess.Sql.Repository
{
    public interface IWorkflowDocumentRepository : IStoreRepository<WorkflowDocument>
    {
    }
}
