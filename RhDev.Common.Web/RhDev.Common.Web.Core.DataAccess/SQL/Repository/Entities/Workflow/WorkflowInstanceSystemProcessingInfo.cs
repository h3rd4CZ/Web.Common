using System;
using System.ComponentModel.DataAnnotations;

namespace RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow
{
    public class WorkflowInstanceSystemProcessingInfo : StoreEntity, IDataStoreEntity
    {
        public int Id { get; set; }
        public WorkflowInstanceSystemProcessingInfoItemType ItemType { get; set; }
        public DateTime Stamp { get; set; }
        [MaxLength(512)]
        public string? Header { get; set; }
        public string? Message { get; set; }
        public WorkflowInstance WorkflowInstance { get; set; }
        public int WorkflowInstanceId { get; set; }

        public override string Identifier => throw new NotImplementedException();
    }
}
