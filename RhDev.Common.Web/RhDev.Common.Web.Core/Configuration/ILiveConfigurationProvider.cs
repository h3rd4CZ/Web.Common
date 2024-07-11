using RhDev.Common.Web.Core.Composition;

namespace RhDev.Common.Web.Core.Configuration
{
    public interface ILiveConfigurationProvider : IAutoregisteredService
    {
        T GetConfiguration<T>() where T : IApplicationConfigurationSection, new();
    }
}
