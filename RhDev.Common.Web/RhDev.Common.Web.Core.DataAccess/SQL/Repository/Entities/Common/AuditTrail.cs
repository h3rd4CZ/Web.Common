using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel.DataAnnotations;

namespace RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Common
{
    public class AuditTrail : StoreEntity, IDataStoreEntity
    {
        public int Id { get; set; }
        [MaxLength(450)]
        public string? UserId { get; set; }
        public AuditType AuditType { get; set; }
        [MaxLength(450)]
        public string? TableName { get; set; }
        public DateTime DateTime { get; set; }
        [MaxLength(32768)]
        public Dictionary<string, object?>? OldValues { get; set; }
        [MaxLength(32768)]
        public Dictionary<string, object?>? NewValues { get; set; }
        [MaxLength(4096)]
        public List<string>? AffectedColumns { get; set; }
        [MaxLength(2048)]
        public Dictionary<string, object> PrimaryKey { get; set; } = new();

        [MaxLength(4096)]
        public List<PropertyEntry> TemporaryProperties { get; } = new();
        public bool HasTemporaryProperties => TemporaryProperties.Any();

        public override string Identifier => Id.ToString();
    }

}
