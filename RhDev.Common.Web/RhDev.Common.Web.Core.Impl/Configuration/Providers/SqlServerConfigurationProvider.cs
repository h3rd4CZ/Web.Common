using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using RhDev.Common.Web.Core.Composition.Factory;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Configuration;
using RhDev.Common.Web.Core.Security;

namespace RhDev.Common.Web.Core.Impl.Configuration.Providers
{
    public class SqlServerConfigurationProvider : ConfigurationProvider, IDisposable
    {
        private readonly SqlServerConfigurationSource _source;
        private IDisposable _changeTokenRegistration;

        public SqlServerConfigurationProvider(
            SqlServerConfigurationSource source)
        {
            _source = source;
            if (_source.SqlServerWatcher is not null)
            {
                _changeTokenRegistration = ChangeToken.OnChange(
                    () => _source.SqlServerWatcher.Watch(),
                    Load
                );
            }
        }

        public override void Set(string key, string value)
        {
            base.Set(key, value);
        }

        public override void Load()
        {
            var services = ApplicationContainerFactory.Root;

            if (services is null) return;

            var ctx = services.TryGetInstance<DbContext>();

            if (ctx is null) return;

            var set = ctx.Set<ApplicationUserSettings>();

            var settings = set
                .AsNoTracking()
                .ToList();

            
            var dic = settings.ToDictionary(s => s.Key, s => GetValue(s.Value));

            Data = dic;
        }

        private string? GetValue(string value)
        {
            if(string.IsNullOrWhiteSpace(value)) return value;

            if(value.StartsWith(UserConfigurationProvider.SafeStringPrefix))
            {
                return StringEncryptor.Decrypt( value.Substring(UserConfigurationProvider.SafeStringPrefix.Length));
            }
            else
            {
                return value;
            }
        }

        public void Dispose()
        {
            _changeTokenRegistration?.Dispose();
            _source?.SqlServerWatcher?.Dispose();
        }
    }
}
