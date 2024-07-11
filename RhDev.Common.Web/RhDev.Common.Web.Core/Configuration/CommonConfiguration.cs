using RhDev.Common.Web.Core.Configuration.ConfigEntities;
using System.ComponentModel.DataAnnotations;

namespace RhDev.Common.Web.Core.Configuration
{
    public class CommonConfiguration : IApplicationConfigurationSection
    {
        public const string TIME_FORMAT = "HH:mm";
        public const string DATE_FORMAT = "yyyy-MM-dd";

        public string Path => $"Common";

        public IApplicationConfigurationSection? Parent => default;

        public const string PATH_APP_OPTIONSRELOADINSECONDS = "App:OptionsReloadInSeconds";

        public int OptionsReloadInSeconds { get; set; } = 60;
        public string BaseUri { get; set; }

        public ExpirationCacheConfiguration CacheConfiguration { get; set; } = new ExpirationCacheConfiguration();

        [ValidateComplexType]
        public IdentityConfiguration Identity { get; set; } = new IdentityConfiguration();

        [ValidateComplexType]
        public SmsGateConfiguration SmsGate { get; set; } = new SmsGateConfiguration();

        [ValidateComplexType]
        public SmtpServerConfiguration SmtpServer { get; set; } = new SmtpServerConfiguration();
        [ValidateComplexType]
        public QueueServiceConfiguration QueueService { get; set; } = new QueueServiceConfiguration();
        [ValidateComplexType]
        public PrivacySettingsConfigurtation PrivacySettings { get; set; } = new PrivacySettingsConfigurtation();
        public static IApplicationConfigurationSection Get => new CommonConfiguration();
    }
}
