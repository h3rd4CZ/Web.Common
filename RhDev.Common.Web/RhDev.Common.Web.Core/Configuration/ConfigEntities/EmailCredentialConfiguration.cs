using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.Security;
using RhDev.Common.Web.Resources.Common;
using System.ComponentModel.DataAnnotations;

namespace RhDev.Common.Web.Core.Configuration.ConfigEntities
{
    public class EmailCredentialConfiguration : IApplicationConfigurationSection
    {
        public string Path => $"{Parent.Path}:Credential";

        public IApplicationConfigurationSection Parent => SmtpServerConfiguration.Get;

        [Display(ResourceType = typeof(FormsAnnotations), Name = "UserName")]
        public string UserName { get; set; }

        [Display(ResourceType = typeof(FormsAnnotations), Name = "Password")]
        [SafeString]
        public string Password { get; set; }

        [Display(ResourceType = typeof(FormsAnnotations), Name = "Domain")]
        public string Domain { get; set; }

        public bool ValidCredential => !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(Domain);
    }
}
