using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Resources.Common;
using System.ComponentModel.DataAnnotations;

namespace RhDev.Common.Web.Core.Configuration.ConfigEntities
{
    public class SmtpServerConfiguration : IApplicationConfigurationSection
    {
        public string Path => $"{Parent.Path}:SmtpServer";
        public IApplicationConfigurationSection Parent => CommonConfiguration.Get;

        [Display(ResourceType = typeof(FormsAnnotations), Name = "EmailAddress")]
        public string Address { get; set; }

        [Display(ResourceType = typeof(FormsAnnotations), Name = "Sender")]
        public string Sender { get; set; }

        [Display(ResourceType = typeof(FormsAnnotations), Name = "BypassOnFailure")]
        public bool BypassOnFailure { get; set; }

        [Display(ResourceType = typeof(FormsAnnotations), Name = "Subject")]
        public string Subject { get; set; }

        [Display(ResourceType = typeof(FormsAnnotations), Name = "Port")]
        public int Port { get; set; } = 25;

        [Display(ResourceType = typeof(FormsAnnotations), Name = "Timeout")]
        public int TimeoutInSeconds { get; set; } = 100;
                
        [Display(ResourceType = typeof(FormsAnnotations), Name = "UseDefaultCredentials")]
        public bool UseDefaultCredentials { get; set; } = false;

        public EmailCredentialConfiguration Credential { get; set; } = new ();

        public static IApplicationConfigurationSection Get => new SmtpServerConfiguration();
    }
}
