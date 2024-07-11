using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.DataStore;

namespace RhDev.Common.Web.Core.DataAccess.SQL
{
    public interface IDataStoreAcessRepositoryFactory : IAutoregisteredService
    {
        IStoreRepository<TEntity> GetAutoregisterStoreRepositoryForEntity<TEntity>() where TEntity : class, IDataStoreEntity;
        [Obsolete("This method use nested container to inject primitive type values and there can be memory with disposable objects in services, because nested containers are not implicitly disposable. Use GetDomainQueryableStoreRepository method instead")]
        TStoreRepository GetDomainQueryableStoreRepositoryUsingNestedContainer<TStoreRepository>(RepositoryCacheStrategy repositoryCacheStrategy = RepositoryCacheStrategy.AlwaysBypass) where TStoreRepository : IDataStoreRepository;
        TStoreRepository GetDomainQueryableStoreRepository<TStoreRepository>(RepositoryCacheStrategy repositoryCacheStrategy = RepositoryCacheStrategy.AlwaysBypass) where TStoreRepository : IDataStoreRepository;
    }
}
