using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Workflow;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow
{
    public enum TransitionRequestType
    {
        Unknown = 0,
        User = 1,
        System = 1 << 1
    }
    public class WorkflowTransitionRequest : StoreEntity
    {
        public int Id { get; set; }
        public int? CompanyBranchId { get; set; }
        public WorkflowTransitionRequestPayload Payload { get; set; }
        public TransitionTaskStatus State { get; set; }
        public DateTime? Finished { get; set; }
        public string? ResultData { get; set; }
        public Guid TransitionId { get; set; }
        [MaxLength(512)]
        public string? Title { get; set; }
        public double TranDuration { get; set; }
        public double RepeatCount { get; set; }
        public string? LastInitiatorId { get; set; }
        [MaxLength(256)]
        public string? Source { get; set; }
        [MaxLength(256)]
        public string? Workflow { get; set; }
        public TransitionRequestType TransitionType { get; set; }
        [MaxLength(128)]
        public string? DocumentReference { get; set; }

        public override string Identifier => Id.ToString();

        [JsonIgnore]
        public bool HasBeenCreated => Id > 0;

        public override string ToString() => $"ID : {Id}, Title : {Title}, reference : {DocumentReference}";
    }
}
