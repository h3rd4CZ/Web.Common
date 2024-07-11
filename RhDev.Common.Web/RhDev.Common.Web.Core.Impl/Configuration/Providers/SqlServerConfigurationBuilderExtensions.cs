using Microsoft.Extensions.Configuration;
using RhDev.Common.Web.Core.Impl.Utils;

namespace RhDev.Common.Web.Core.Impl.Configuration.Providers
{
    public static class SqlServerConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddSqlServer(this IConfigurationBuilder builder, string connectionString, TimeSpan? refreshInterval = default)
        {
            return builder.Add(
                new SqlServerConfigurationSource(connectionString,
                    refreshInterval.HasValue
                    ? new SqlServerPeriodicalWatcher(refreshInterval.Value)
                    : default));
        }
    }
}
