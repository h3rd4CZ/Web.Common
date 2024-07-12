using Microsoft.AspNetCore.Identity;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace RhDev.Customer.Component.Core.Impl.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? City { get; set; }
                
        public ICollection<City> AuthoredCities { get; set; } = new List<City>();
        public ICollection<City> ModifiedCities { get; set; } = new List<City>();

        public ICollection<ApplicationUser> AuthoredUsers { get; set; } = new List<ApplicationUser>();
        public ICollection<ApplicationUser> ModifiedUsers { get; set; } = new List<ApplicationUser>();
               
    }
}
