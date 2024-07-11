using RhDev.Common.Web.Core.Configuration;

namespace RhDev.Customer.Component.Core.Impl.Options
{
    public class CustomerOptions : IApplicationConfigurationSection
    {
        public string Path => "Customer";
        public int Size { get; set; }
        public string? Name { get; set; }
        public IApplicationConfigurationSection? Parent => default;
    }
}
