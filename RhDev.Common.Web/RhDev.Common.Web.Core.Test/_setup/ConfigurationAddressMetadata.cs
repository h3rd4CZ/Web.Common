using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.Security;

namespace RhDev.Common.Web.Core.Test._setup
{
    public class ConfigurationAddressMetadata : IApplicationConfigurationSection
    {
        public int Population { get; set; }
        public decimal QoL { get; set; }
        public ConfigurationAddressMetadataDescription Description { get; set; } = new();

        public string Path => "";
        public IApplicationConfigurationSection? Parent => default!;
        public static IApplicationConfigurationSection Get => new ConfigurationAddressMetadata();
    }

    public class ConfigurationAddressMetadataDescription : IApplicationConfigurationSection
    {
        [SafeString]
        public string Created { get; set; }
                
        public string Path => $"{Parent!.Path}:Description";
        public IApplicationConfigurationSection? Parent => ConfigurationAddressMetadata.Get;
    }
}
