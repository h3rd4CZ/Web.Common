#nullable disable
using Microsoft.AspNetCore.Identity;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow
{
    public class WorkflowInstance : StoreEntity, IDataStoreEntity
    {
        public int Id { get; set; }

        public int WorkflowDocumentId { get; set; }
        public WorkflowDocument WorkflowDocument { get; set; }

        [MaxLength(512)]
        public string InstanceId { get; set; }
        [MaxLength(1024)]
        public string Name { get; set; }
        [MaxLength(64)]
        public string RunVersion { get; set; }
        public Guid? CurrentTaskGroupId { get; set; }
        public DateTime Started { get; set; }
        public DateTime? Finished { get; set; }
        [MaxLength(512)]
        public string WorkflowState { get; set; }
        public bool WorkflowStateSystem { get; set; }
        public string InitiatorId { get; set; }
        public byte[] WorkflowDefinition { get; set; }
        public DateTime? LastSystemProcessing { get; set; }
        public bool IsFailed { get; set; }

        public ICollection<WorkflowTask> Tasks { get; set; }
        public ICollection<WorkflowTransitionLog> TransitionLogs { get; set; }
        public ICollection<WorkflowProperty> WorkflowProperties { get; set; }
        public ICollection<WorkflowInstanceSystemProcessingInfo> SystemProcessingInfoItems { get; set; }
        public ICollection<WorkflowInstanceHistory> WorkflowInstanceHistory { get; set; }

        public override string Identifier => Id.ToString();

        public override string ToString()
        {
            return $"Workflow instance ID : {Id}, Workflow name : {Name}, version : {RunVersion}";
        }

    }
}
