using RhDev.Common.Web.Core;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Common;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace RhDev.Customer.Component.Core.Impl.Data
{
    public class City : StoreEntity, IAuditTrailEntity<ApplicationUser>, ISoftDelete
    {
        public int Id { get; set; }

        public string? Title { get; set; }
        public long Population { get; set; }

        public override string Identifier => Id.ToString();

        public DateTime? Created { get; set; }
        public ApplicationUser? CreatedBy { get; set; }
        public string? CreatedById { get; set; }
        public DateTime? LastModified { get; set; }

        public ApplicationUser? LastModifiedBy { get; set; }
        public string? LastModifiedById { get; set; }
        public DateTime? Deleted { get; set; }
        public string? DeletedBy { get; set; }
        [Timestamp]
        public byte[] Version { get; set; }
    }
}
