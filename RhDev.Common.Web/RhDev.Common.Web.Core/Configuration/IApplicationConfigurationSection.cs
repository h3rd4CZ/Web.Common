namespace RhDev.Common.Web.Core.Configuration
{
    public interface IApplicationConfigurationSection
    {
        public string Path { get; }
        public IApplicationConfigurationSection? Parent { get; }
    }

}
