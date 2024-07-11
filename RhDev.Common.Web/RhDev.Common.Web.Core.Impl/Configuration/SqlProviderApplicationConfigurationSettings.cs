using Microsoft.Extensions.Configuration;
using RhDev.Common.Web.Core.DataAccess.Sql.DependencyInjection;
using RhDev.Common.Web.Core.Impl.Configuration.Providers;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Web.Core.Impl.Configuration
{
    public class SqlProviderApplicationConfigurationSettings
    {
        public static void ConfigureSqlConfigurationProvider(IConfigurationRoot configurationRoot, IConfigurationBuilder configurationBuilder, TimeSpan optionsReloadInSeconds = default, string connStringKey = default)
        {
            Guard.NotNull(configurationRoot, nameof(configurationBuilder));

            Guard.NotNull(configurationBuilder, nameof(configurationBuilder));

            var connString = string.IsNullOrWhiteSpace(connStringKey)
                ? configurationRoot.GetConnectionString(Constants.DEFAULTCONNECTION_KEY)
                : configurationRoot.GetValue<string>(connStringKey);

            Guard.StringNotNullOrWhiteSpace(connString);

            configurationBuilder.AddSqlServer(connString, optionsReloadInSeconds);
        }
    }
}
