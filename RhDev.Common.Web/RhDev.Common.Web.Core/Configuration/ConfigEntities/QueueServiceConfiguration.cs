using RhDev.Common.Web.Core.Configuration;

namespace RhDev.Common.Web.Core.Configuration.ConfigEntities
{
    public class QueueServiceConfiguration : IApplicationConfigurationSection
    {
        public string Path => $"{Parent.Path}:QueueService";

        public IApplicationConfigurationSection Parent => CommonConfiguration.Get;

        public int QueueCapacity { get; set; } = 1000;

        public int Workers { get; set; } = 5;
    }
}
