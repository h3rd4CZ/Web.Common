using Microsoft.Extensions.Configuration;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.Impl.Configuration;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Web.Core.Impl.Host
{
    public static class IConfigurationBuilderExtensions
    {
        public static void ConfigureConfiguration(this IConfigurationBuilder configuration, string[] prefixes = default!)
        {
            if (prefixes is not null)
            {
                foreach (var prefix in prefixes.Where(p => !string.IsNullOrWhiteSpace(p)))
                {
                    configuration.AddEnvironmentVariables(prefix);
                }
            }

            var cfgBuilt = configuration.Build();
                        
            var reloadInSecondsPath = ConfigurationUtils.GetPathConfigurationProperty<CommonConfiguration>(a => a.OptionsReloadInSeconds);

            Guard.StringNotNullOrWhiteSpace(reloadInSecondsPath);

            var optionsReloadInterval = cfgBuilt.GetSection(reloadInSecondsPath)?.Value;

            SqlProviderApplicationConfigurationSettings
            .ConfigureSqlConfigurationProvider(
                cfgBuilt,
                configuration,
                int.TryParse(optionsReloadInterval, out int interval) ? TimeSpan.FromSeconds(interval) : default);

        }
    }
}
