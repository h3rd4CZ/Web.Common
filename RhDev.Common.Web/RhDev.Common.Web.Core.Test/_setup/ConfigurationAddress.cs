using RhDev.Common.Web.Core.Configuration;

namespace RhDev.Common.Web.Core.Test._setup
{
    public class ConfigurationAddress : IApplicationConfigurationSection
    {
        public string City { get; set; }
        public string Zip { get; set; }
        public int MyProperty { get; set; }
        public string Path => "";

        public ConfigurationAddressMetadataDescription Description { get; set; } = new();

        public List<string> Favs { get; set; } = new();

        public List<ConfigurationAddressMetadata> Metadata { get; set; } = new();

        public IApplicationConfigurationSection? Parent => default!;
    }
}
