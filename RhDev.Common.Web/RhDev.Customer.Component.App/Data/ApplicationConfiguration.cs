using RhDev.Common.Web.Core.Configuration;

namespace RhDev.Customer.Component.App.Data
{
    public class ApplicationConfiguration : IApplicationConfigurationSection
    {
        public string Path => "App";

        public int Timeout { get; set; }

        public IApplicationConfigurationSection? Parent => default;
    }
}
