using Microsoft.Extensions.Configuration;
using RhDev.Common.Web.Core.Configuration;

namespace RhDev.Common.Web.Core.Impl.Configuration
{
    public class LiveConfigurationProvider : ILiveConfigurationProvider
    {
        private readonly IConfiguration configuration;

        public LiveConfigurationProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        
        public T GetConfiguration<T>() where T : IApplicationConfigurationSection, new()
        {
            var config = GetRefreshedConfiguration();

            var sectionObject = new T();

            return config.GetSection(sectionObject.Path).Get<T>()! ?? new T();
        }

        private IConfiguration GetRefreshedConfiguration()
        {
            ((IConfigurationRoot)configuration).Reload();

            return configuration;
        }
    }
}
