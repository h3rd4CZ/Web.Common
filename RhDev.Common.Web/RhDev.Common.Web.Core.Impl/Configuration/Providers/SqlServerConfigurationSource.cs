using Microsoft.Extensions.Configuration;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Web.Core.Impl.Configuration.Providers
{
    public class SqlServerConfigurationSource : IConfigurationSource
    {
        private readonly string connectionString;

        private readonly ISqlServerWatcher sqlServerWatcher;

        public SqlServerConfigurationSource(string connectionString, ISqlServerWatcher sqlServerWatcher)
        {
            this.connectionString = connectionString;
            this.sqlServerWatcher = sqlServerWatcher;
        }

        public string ConnectionString => connectionString;

        public ISqlServerWatcher SqlServerWatcher => sqlServerWatcher;

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new SqlServerConfigurationProvider(this);    
        }
    }
}
