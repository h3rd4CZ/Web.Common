using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Resources.Common;
using System.ComponentModel.DataAnnotations;

namespace RhDev.Common.Web.Core.Configuration.ConfigEntities
{
    public class SmsGateConfiguration : IApplicationConfigurationSection
    {
        public string Path => $"{Parent.Path}:SmsGate";
        public IApplicationConfigurationSection Parent => CommonConfiguration.Get;

        [Display(ResourceType = typeof(FormsAnnotations), Name = "SmsAddress")]
        public string Address { get; set; }

        [Display(ResourceType = typeof(FormsAnnotations), Name = "ByPassFailure")]
        public bool ByPassFailure { get; set; }

        [Display(ResourceType = typeof(FormsAnnotations), Name = "AccessToken")]
        public string AccessToken { get; set; }

        [Display(ResourceType = typeof(FormsAnnotations), Name = "Queue")]
        public string Queue { get; set; }

        public List<string> PhoneCountryCodes { get; set; } = new List<string> { };
    }
}
