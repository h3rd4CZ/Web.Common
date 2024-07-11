using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.DataAccess.SQL;

namespace RhDev.Common.Web.Core.DataAccess.Caching
{
    public interface IRepositoryCacheService<TEntity> : IService 
        where TEntity : StoreEntity, IDataStoreEntity
    {
        Task<TEntity> GetSingleValueAsync(CacheKey key, Func<Task<TEntity>> valueProvider);
        Task<IList<TEntity>> GetCollectionValueAsync(CacheKey key, Func<Task<IList<TEntity>>> valueProvider);
        Task<IList<TProjectedEntity>> GetProjectedCollectionValueAsync<TProjectedEntity>(CacheKey key, Func<Task<IList<TProjectedEntity>>> valueProvider);
        Task<PaginatedResult<TProjectedEntity>> GetPaginatedProjectedValueAsync<TProjectedEntity>(CacheKey key, Func<Task<PaginatedResult<TProjectedEntity>>> valueProvider);
        Task InvalidateEntityAsync();
        Task ProcessDeleteAsync(string identifier);
        Task ProcessUpdateAsync(string identifier);
        Task ProcessCreateAsync(TEntity entity);

    }
}
