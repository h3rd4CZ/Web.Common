using Microsoft.AspNetCore.Identity;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Core.Test.Data
{
    public class TestWorkflowMetadata : DocumentWorkflowMetadata
    {
        public override string DocumentReference => "DOC-X123";

        public override string Identifier => Id.ToString();

        public string? InvoiceNumber { get; internal set; }
        public string? DocumentNumber { get; internal set; }
        public string? State { get; internal set; }
        public DateTime Created { get; set; }
        public IdentityUser CreatedBy { get; set; }
        public string CreatedById { get; set; }
        public bool MinedSuccessfully { get; internal set; }
    }
}
