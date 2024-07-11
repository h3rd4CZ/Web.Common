using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.DataAccess.SQL;

namespace RhDev.Common.Web.Core.DataAccess.Caching
{
    public abstract class RepositoryCacheServiceBase<TEntity> where TEntity : StoreEntity, IDataStoreEntity
    {
        private readonly RepositoryCacheSettings repositoryCacheSettings;
        private readonly IRepositoryCacheService<TEntity> repositoryCacheService;

        public RepositoryCacheServiceBase(RepositoryCacheSettings repositoryCacheSettings, IRepositoryCacheService<TEntity> repositoryCacheService)
        {
            this.repositoryCacheSettings = repositoryCacheSettings ?? RepositoryCacheSettings.ForCacheStrategy(RepositoryCacheStrategy.AlwaysBypass);
            this.repositoryCacheService = repositoryCacheService;
        }

        protected async Task<TEntity> UseSingleItemCacheServiceIfRequired(CacheKey key, Func<Task<TEntity>> valueProvider)
        {
            if (repositoryCacheSettings is not null && repositoryCacheSettings.CacheStrategy == RepositoryCacheStrategy.ReadOnly) return await repositoryCacheService.GetSingleValueAsync(key, valueProvider);

            return await valueProvider();
        }

        protected async Task<IList<TEntity>> UseItemCollectionCacheServiceIfRequired(CacheKey key, Func<Task<IList<TEntity>>> valueProvider)
        {
            if (repositoryCacheSettings is not null && repositoryCacheSettings.CacheStrategy == RepositoryCacheStrategy.ReadOnly) return await repositoryCacheService.GetCollectionValueAsync(key, valueProvider);

            return await valueProvider();   
        }

        protected async Task<IList<TProjectedEntity>> UseProjectedItemCollectionCacheServiceIfRequired<TProjectedEntity>(CacheKey key, Func<Task<IList<TProjectedEntity>>> valueProvider)
        {
            if (repositoryCacheSettings is not null && repositoryCacheSettings.CacheStrategy == RepositoryCacheStrategy.ReadOnly) return await repositoryCacheService.GetProjectedCollectionValueAsync(key, valueProvider);

            return await valueProvider();
        }

        protected async Task<PaginatedResult<TProjectedEntity>> UsePaginatedProjectedCacheServiceIfRequired<TProjectedEntity>(CacheKey key, Func<Task<PaginatedResult<TProjectedEntity>>> valueProvider)
        {
            if (repositoryCacheSettings is not null && repositoryCacheSettings.CacheStrategy == RepositoryCacheStrategy.ReadOnly) return await repositoryCacheService.GetPaginatedProjectedValueAsync(key, valueProvider);

            return await valueProvider();
        }

        protected async Task ItemCacheServiceOnModify(TEntity entity)
        {
            var taskToDo = repositoryCacheSettings.CacheStrategy switch
            {
                RepositoryCacheStrategy.InvalidateAffectedOnWrite => repositoryCacheService.ProcessUpdateAsync(entity.Identifier),
                RepositoryCacheStrategy.InvalidateAllOnWrite => repositoryCacheService.InvalidateEntityAsync(),
                _ => Task.CompletedTask
            };

            await taskToDo;
        }

        protected async Task ItemCacheServiceOnDelete(TEntity entity)
        {
            var taskToDo = repositoryCacheSettings.CacheStrategy switch
            {
                RepositoryCacheStrategy.InvalidateAffectedOnWrite => repositoryCacheService.ProcessDeleteAsync(entity.Identifier),
                RepositoryCacheStrategy.InvalidateAllOnWrite => repositoryCacheService.InvalidateEntityAsync(),
                _ => Task.CompletedTask
            };

            await taskToDo;
        }

        protected async Task ItemCacheServiceOnCreate(TEntity entity)
        {
            var taskToDo = repositoryCacheSettings.CacheStrategy switch
            {
                RepositoryCacheStrategy.InvalidateAffectedOnWrite => repositoryCacheService.ProcessCreateAsync(entity),
                RepositoryCacheStrategy.InvalidateAllOnWrite => repositoryCacheService.ProcessCreateAsync(entity),
                _ => Task.CompletedTask
            };

            await taskToDo;
        }

        protected CacheKey BuildKey(string methodName, params object[] methodParameterValues)
        {
            var repositoryTypeKey = CacheKey.ForValuesWhere("REPOSITORY", GetType().Name);
            var methodNameKey = CacheKey.ForValuesWhere("METHOD", methodName);
            var parametersValueKey = CacheKey.ForValuesWhere("PARAMS", string.Join("$$$", methodParameterValues is null || methodParameterValues.Length == 0 ? new string[] { "_" } : methodParameterValues));

            return
                repositoryTypeKey
                .CombineWith(methodNameKey)
                .CombineWith(parametersValueKey);
        }
    }
}
