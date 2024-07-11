using RhDev.Common.Web.Core.DataAccess.Sql.Repository;
using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.DataAccess.Caching;
using RhDev.Common.Web.Core.DataAccess.SQL;

namespace RhDev.Customer.Component.Core.Impl.Data
{
    public class CityDataStore : DataStoreEntityRepositoryBase<City, ApplicationDbContext>, ICityDataStore,
        IAutoRegisterStoreRepository

    {
        public CityDataStore(
            RepositoryCacheSettings repositoryCacheSettings,
            IRepositoryCacheService<City> repositoryCacheService,
            IDatabaseAccessRepositoryFactory<ApplicationDbContext> storeFactory) : base(repositoryCacheSettings, repositoryCacheService, storeFactory) { }
    }
}
