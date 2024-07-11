using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Workflow;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow
{
    [Index(nameof(WorkflowAssignedTo), IsUnique = false, Name = $"IX_{nameof(DocumentWorkflowMetadata)}_{nameof(WorkflowAssignedTo)}")]
    public abstract class DocumentWorkflowMetadata : StoreEntity, IWorkflowDocument
    {
        public int Id { get; set; }

        [WorkflowUserField]
        public string? WorkflowAssignedTo { get; set; }

        public DateTime? WorkflowAssignedAt { get; set; }
                
        public WorkflowDocumentIdentificator WorkflowIdentificator => new(Id, GetType().AssemblyQualifiedName!);

        public int WorkflowDocumentId { get; set; }

        [NotMapped]
        public abstract string DocumentReference { get; }

        [NotMapped]
        public string[] ExplicitIncludes => new string[0]
        {
        };

        public ICollection<WorkflowTask> WorkflowTasks { get; set; } = new List<WorkflowTask>();
    }
}
