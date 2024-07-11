using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Configuration
{
    [Index(nameof(Key), Name = "IX_Key", IsUnique = true)]
    public class ApplicationUserSettings : StoreEntity, IDataStoreEntity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Key { get; set; }
        [MaxLength(4096)]
        public string Value { get; set; }
        [MaxLength(450)]
        public string ChangedBy { get; set; }
        public DateTimeOffset Changed { get; set; }
        public override string Identifier => Id.ToString();
    }
}