namespace RhDev.Common.Web.Core.Configuration.ConfigEntities
{
    public class PrivacySettingsConfigurtation : IApplicationConfigurationSection
    {
        public string Path => $"{Parent!.Path}:PrivacySettings";              

        /// <summary>
        ///     Gets or sets a value indicating whether the client IP addresses should be logged.
        /// </summary>
        public bool LogClientIpAddresses { get; set; } = false;

        /// <summary>
        ///     Gets or sets a value indicating whether the client agents (user agents) should be logged.
        /// </summary>
        public bool LogClientAgents { get; set; } = false;
                

        public IApplicationConfigurationSection? Parent => CommonConfiguration.Get;
    }
}
