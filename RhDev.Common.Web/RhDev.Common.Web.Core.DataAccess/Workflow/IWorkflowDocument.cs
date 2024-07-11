using RhDev.Common.Web.Core.DataAccess.SQL;

namespace RhDev.Common.Web.Core.DataAccess.Workflow
{
    public interface IWorkflowDocument : IDataStoreEntity
    {
        public string[] ExplicitIncludes { get; }
        public string? WorkflowAssignedTo { get; set; }
        public int WorkflowDocumentId { get; set; }
        public string DocumentReference { get; }
    }
}
