namespace RhDev.Common.Web.Core.Configuration.ConfigEntities
{
    public class ExpirationCacheConfiguration : IApplicationConfigurationSection
    {
        public string Path => $"{Parent.Path}:CacheConfiguration";
        public IApplicationConfigurationSection Parent => CommonConfiguration.Get;
        public int ExpirationDurationInMinutes { get; set; } = 1440;
        public static IApplicationConfigurationSection Get => new ExpirationCacheConfiguration();
    }
}
