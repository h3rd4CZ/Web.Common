using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.DataAccess.Caching;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Utils;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Stores.Utils;

namespace RhDev.Common.Web.Core.DataAccess.Sql.Repository.Stores.Utils
{
    public class DayOffDataStore : DataStoreEntityRepositoryBase<DayOff, DbContext>, IDayOffDataStore,
        IAutoRegisterStoreRepository

    {
        public DayOffDataStore(
            RepositoryCacheSettings repositoryCacheSettings,
            IRepositoryCacheService<DayOff> repositoryCacheService,
            IDatabaseAccessRepositoryFactory<DbContext> storeFactory) : base(repositoryCacheSettings, repositoryCacheService, storeFactory) { }
    }
}
