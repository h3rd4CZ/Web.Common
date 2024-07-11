using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.DataAccess.Caching;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Utils;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Stores.Configuration;

namespace RhDev.Common.Web.Core.DataAccess.Sql.Repository.Stores.Utils
{
    public class LoggerDataStore : DataStoreEntityRepositoryBase<Logger, DbContext>, 
        ILoggerDataStore,
        IAutoRegisterStoreRepository

    {
        public LoggerDataStore(
            RepositoryCacheSettings repositoryCacheSettings,
            IRepositoryCacheService<Logger> repositoryCacheService,
            IDatabaseAccessRepositoryFactory<DbContext> storeFactory) : base(repositoryCacheSettings, repositoryCacheService, storeFactory) { }
    }
}
