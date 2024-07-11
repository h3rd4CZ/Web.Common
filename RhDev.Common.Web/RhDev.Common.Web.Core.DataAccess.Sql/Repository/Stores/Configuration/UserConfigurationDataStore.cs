using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.DataAccess.Caching;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Configuration;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Stores.Configuration;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Web.Core.DataAccess.Sql.Repository.Stores.Configuration
{

    public class UserConfigurationDataStore : DataStoreEntityRepositoryBase<ApplicationUserSettings, DbContext>, IUserConfigurationDataStore,
        IAutoRegisterStoreRepository

    {
        public UserConfigurationDataStore(
            RepositoryCacheSettings repositoryCacheSettings,
            IRepositoryCacheService<ApplicationUserSettings> repositoryCacheService,
            IDatabaseAccessRepositoryFactory<DbContext> storeFactory) : base(repositoryCacheSettings, repositoryCacheService, storeFactory) { }

        public async Task<ApplicationUserSettings> ReadSettingsKeyAsync(string key)
        {
            Guard.StringNotNullOrWhiteSpace(key, nameof(key));

            var data = await ReadAsync(s => s.Key == key);

            return data.Any() ? data[0] : default;
        }
    }
}
