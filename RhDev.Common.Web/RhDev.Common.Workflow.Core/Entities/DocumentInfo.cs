using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.Workflow;
using System;

namespace RhDev.Common.Workflow.Entities
{
    [Serializable]
    public class DocumentInfo
    {
        public WorkflowDocumentIdentificator WorkflowDocumentIdentificator { get; set; }
        public int Id { get; set; }

        public static DocumentInfo Fill(WorkflowDocument document)
        {
            var di = new DocumentInfo()
            {
                Id = document.Id,
                WorkflowDocumentIdentificator = document.WorkflowDocumentIdentificator
            };

            return di;
        }
    }
}
