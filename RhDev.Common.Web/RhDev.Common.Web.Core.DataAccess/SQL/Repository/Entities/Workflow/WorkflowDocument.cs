using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.Workflow;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow
{
    public class WorkflowDocument : StoreEntity, IDataStoreEntity
    {
        public ICollection<WorkflowInstance> WorkflowInstances { get; set; }

        public int Id { get; set; }

        [MaxLength(1024)]
        public WorkflowDocumentIdentificator WorkflowDocumentIdentificator { get; set; }

        public int UnitEntityId { get; set; }

        [MaxLength(128)]
        public string DocumentReference { get; set; }

        public override string Identifier => Id.ToString();

        public override string ToString() => $"Workflow Document ID : {WorkflowDocumentIdentificator.entityId}, type : {WorkflowDocumentIdentificator.typeName}";
    }
}
