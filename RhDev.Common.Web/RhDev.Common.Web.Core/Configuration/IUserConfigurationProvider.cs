using RhDev.Common.Web.Core.Composition;
using System.Linq.Expressions;

namespace RhDev.Common.Web.Core.Configuration
{
    public interface IUserConfigurationProvider : IAutoregisteredService
    {
        Task WriteConfigurationAsync<TConfiguration>(
            TConfiguration configuration,
            string userId, string configurationContainerPrefix = default!) where TConfiguration : IApplicationConfigurationSection;

        Task WriteConfigurationPropertyAsync<TConfiguration>(
            TConfiguration configuration,
            Expression<Func<TConfiguration, object>> configurationProperty,
            string userId) where TConfiguration : IApplicationConfigurationSection;
    }
}
