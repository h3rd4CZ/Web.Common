using System.ComponentModel.DataAnnotations;

namespace RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow
{
    public class WorkflowDefinition : StoreEntity, IDataStoreEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Title { get; set; }

        public string? Comment { get; set; }       
        
        public override string Identifier => Id.ToString();
    }
}
