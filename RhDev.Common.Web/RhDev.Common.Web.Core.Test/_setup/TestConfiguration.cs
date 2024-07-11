using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.Security;

namespace RhDev.Common.Web.Core.Test._setup
{
    public class TestConfiguration : IApplicationConfigurationSection
    {
        public string Path => "Test";
        public int Delay { get; set; }
        public List<ConfigurationAddress> Addresses { get; set; } = new();
        public TestSettings Settings { get; set; } = new ();
        public IApplicationConfigurationSection? Parent => default!;
        public float Diff { get; set; }
        public long LongData { get; set; }

        public static TestConfiguration Get => new TestConfiguration();
    }

    public class TestSettings : IApplicationConfigurationSection
    {
        public string Path => $"{Parent.Path}:Settings";

        public int Age  { get; set; }
        
        [SafeString]
        public string TestSafeString { get; set; }

        public IApplicationConfigurationSection Parent => TestConfiguration.Get;
    }
}
