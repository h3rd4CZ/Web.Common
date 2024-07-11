using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.DataAccess.Caching;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Configuration;

namespace RhDev.Common.Web.Core.Test._setup
{
    public class UserSettingsDataStore : DataStoreEntityRepositoryBase<ApplicationUserSettings, TestApplicationDatabaseContext>,
        ITestApplicationDataStore,
        IAutoRegisterStoreRepository

    {
        public UserSettingsDataStore(
            RepositoryCacheSettings repositoryCacheSettings,
            IRepositoryCacheService<ApplicationUserSettings> repositoryCacheService,
            IDatabaseAccessRepositoryFactory<TestApplicationDatabaseContext> storeFactory) : base(repositoryCacheSettings, repositoryCacheService, storeFactory) { }
    }
}
