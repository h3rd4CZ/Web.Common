using System.ComponentModel.DataAnnotations;

namespace RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow
{
    public class WorkflowProperty : StoreEntity, IDataStoreEntity
    {
        public int Id { get; set; }
        public int WorkflowInstanceId { get; set; }
        public WorkflowInstance WorkflowInstance { get; set; }
        [MaxLength(256)]
        public string Name { get; set; }
        public string? Value { get; set; }
        [MaxLength(1024)]
        public string Type { get; set; }

        public override string Identifier => Id.ToString();
    }
}
