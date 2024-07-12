using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow
{
    public class WorkflowTransitionLog : StoreEntity, IDataStoreEntity
    {
        public int Id { get; set; }
        [MaxLength(64)]
        public string? TransitionId { get; set; }
        public string? UserId { get; set; }
        [MaxLength(512)]
        public string? FromState { get; set; }
        [MaxLength(512)]
        public string? ToState { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Dictionary<string, object> UserData { get; set; }
        public string? Result { get; set; }
        [MaxLength(1024)]
        public string? Handler { get; set; }
        public string? Data { get; set; }
        public int WorkflowInstanceId { get; set; }
        public WorkflowInstance WorkflowInstance { get; set; }

        public override string Identifier => Id.ToString();
    }
}
