using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.Workflow;
using System;
using System.ComponentModel.DataAnnotations;

namespace RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow
{
    [Index(nameof(AssignedTo), Name = "IX_AssignedTo", IsUnique = false)]
    public class WorkflowTask : StoreEntity, IDataStoreEntity
    {
        public int Id { get; set; }
        public int WorkflowInstanceId { get; set; }
        public WorkflowInstance WorkflowInstance { get; set; }
        public WorkflowDatabaseTaskStatus Status { get; set; }
        [MaxLength(512)]
        public string AssignedTo { get; set; }
        public TaskAssigneeType AssigneeType { get; set; }
        public Guid GroupId { get; set; }
        public string? ResolvedById { get; set; }
        public DateTime? ResolvedOn { get; set; }
        public DateTime AssignedOn { get; set; }
        [MaxLength(4000)]
        public string? UserData { get; set; }
        public DateTime? DueDate { get; set; }
        [MaxLength(1024)]
        public string? Title { get; set; }
        [MaxLength(1024)]
        public string? SelectedTriggerCode { get; set; }
        public WorkflowTaskRespondType TaskRespondeType { get; set; }
                
        public override string Identifier => Id.ToString();
    }
}
