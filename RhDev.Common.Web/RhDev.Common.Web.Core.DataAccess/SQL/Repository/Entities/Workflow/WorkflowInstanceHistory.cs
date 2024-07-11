using System.ComponentModel.DataAnnotations;

namespace RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow
{
    public class WorkflowInstanceHistory : StoreEntity, IDataStoreEntity
    {
        public int Id { get; set; }
        public int WorkflowInstanceId { get; set; }
        public WorkflowInstance WorkflowInstance { get; set; }
        public DateTime Date { get; set; }
        public string UserId { get; set; }
        [MaxLength(512)]
        public string? Event { get; set; }
        [MaxLength(4000)]
        public string? Message { get; set; }
        public override string Identifier => Id.ToString();
    }
}
