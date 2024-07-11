using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.DataAccess.Caching;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.Impl.Timer.Queue;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Web.Core.Impl.Caching
{
    public class RepositoryCacheService<TEntity> : IRepositoryCacheService<TEntity> where TEntity : StoreEntity, IDataStoreEntity
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<RepositoryCacheService<TEntity>> logger;
        private readonly IAsynchronousCache<TEntity> singleValueCache;
        private readonly IBackgroundTaskQueue backgroundTaskQueue;
        private readonly IAsynchronousCache<IList<TEntity>> multipleValueCache;        
        private Func<string, string> queueIdGenerator = g => $"{nameof(RepositoryCacheService<TEntity>)}_{g}";

        public RepositoryCacheService(
            IServiceProvider serviceProvider,
            ILogger<RepositoryCacheService<TEntity>> logger,
            IAsynchronousCache<TEntity> singleValueCache,
            IBackgroundTaskQueue backgroundTaskQueue,
            IAsynchronousCache<IList<TEntity>> multipleValueCache)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.singleValueCache = singleValueCache;
            this.backgroundTaskQueue = backgroundTaskQueue;
            this.multipleValueCache = multipleValueCache;
        }
        public async Task<IList<TEntity>> GetCollectionValueAsync(CacheKey key, Func<Task<IList<TEntity>>> valueProvider)
            => await multipleValueCache.GetOrFetchValueAsync(key, valueProvider);

        public async Task<IList<TProjectedEntity>> GetProjectedCollectionValueAsync<TProjectedEntity>(CacheKey key, Func<Task<IList<TProjectedEntity>>> valueProvider)
        {
            var cache
                = serviceProvider.GetRequiredService<IAsynchronousCache<IList<TProjectedEntity>>>();

            return await cache.GetOrFetchValueAsync(key, valueProvider);
        }

        public async Task<PaginatedResult<TProjectedEntity>> GetPaginatedProjectedValueAsync<TProjectedEntity>(CacheKey key, Func<Task<PaginatedResult<TProjectedEntity>>> valueProvider)
        {
            var cache
                = serviceProvider.GetRequiredService<IAsynchronousCache<PaginatedResult<TProjectedEntity>>>();

            return await cache.GetOrFetchValueAsync(key, valueProvider);
        }

        public async Task<TEntity> GetSingleValueAsync(CacheKey key, Func<Task<TEntity>> valueProvider)
            => await singleValueCache.GetOrFetchValueAsync(key, valueProvider);
                
        public async Task InvalidateEntityAsync()
        {
            await singleValueCache.InvalidateCacheAsync();

            await multipleValueCache.InvalidateCacheAsync();
        }

        public async Task ProcessDeleteAsync(string identifier)
        {
            Guard.StringNotNullOrWhiteSpace(identifier);

            await ProcessUpdateAsync(identifier);
        }

        public async Task ProcessUpdateAsync(string identifier)
        {
            Guard.StringNotNullOrWhiteSpace(identifier);

            await backgroundTaskQueue.QueueBackgroundWorkItemAsync(new QueueWorkItem(queueIdGenerator(Guid.NewGuid().ToString()), async (c) =>
            {
                var singleValueCacheContent = await singleValueCache.GetCacheContentAsync();
                foreach (var item in singleValueCacheContent)
                    if (item.Value.Identifier == identifier) await singleValueCache.InvalidateCacheItemAsync(item.Key);

                var multipleValueCacheContent = await multipleValueCache.GetCacheContentAsync();
                foreach (var item in multipleValueCacheContent)
                    if (item.Value.Any(i => i.Identifier == identifier)) await multipleValueCache.InvalidateCacheItemAsync(item.Key);
            }));
        }

        public Task ProcessCreateAsync(TEntity entity) => InvalidateEntityAsync();
    }
}
