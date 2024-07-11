using Microsoft.AspNetCore.Identity;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace RhDev.Customer.Component.Core.Impl.Data
{
    public class ApplicationUser : IdentityUser, IAuditTrailEntity<ApplicationUser>
    {
        public string? City { get; set; }

        public ApplicationUser? CreatedBy { get; set; }
        public string? CreatedById { get; set; }

        public ApplicationUser? LastModifiedBy { get; set; }
        public string? LastModifiedById { get; set; }

        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }

        public ICollection<City> AuthoredCities { get; set; } = new List<City>();
        public ICollection<City> ModifiedCities { get; set; } = new List<City>();

        public ICollection<ApplicationUser> AuthoredUsers { get; set; } = new List<ApplicationUser>();
        public ICollection<ApplicationUser> ModifiedUsers { get; set; } = new List<ApplicationUser>();
                
        [NotMapped]
        int IDataStoreEntity.Id { get; set; }
    }
}
