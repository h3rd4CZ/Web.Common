using RhDev.Common.Web.Resources.Common;
using System.ComponentModel.DataAnnotations;

namespace RhDev.Common.Web.Core.Configuration.ConfigEntities
{
    public class IdentityConfiguration : IApplicationConfigurationSection
    {
        public string Path => $"{Parent.Path}:Identity";
        public IApplicationConfigurationSection Parent => CommonConfiguration.Get;

        [Range(1, 9999999, ErrorMessageResourceType = typeof(FormsAnnotations), ErrorMessageResourceName = "NotInRange")]
        [Display(ResourceType = typeof(FormsAnnotations), Name = "CookieExpirationinMinutes")]
        public int? CookieExpirationInMinutes { get; set; } = 60;

        [Range(1, 9999999, ErrorMessageResourceType = typeof(FormsAnnotations), ErrorMessageResourceName = "NotInRange")]
        [Display(ResourceType = typeof(FormsAnnotations), Name = "SignoutInactivityIntervalInMinutes")]
        public int? SignoutInactivityIntervalInMinutes { get; set; } = 5;

        public static IdentityConfiguration Get => new IdentityConfiguration();
    }
}
