using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.DataAccess.Caching;
using RhDev.Common.Web.Core.Extensions;
using RhDev.Common.Web.Core.Utils;
using System.Linq.Expressions;

namespace RhDev.Common.Web.Core.DataAccess.SQL
{
    public abstract class DataStoreEntityRepositoryBase<TStoreEntity, TDatabase> : DatabaseRepositoryBase<TStoreEntity, TDatabase>, IStoreRepository<TStoreEntity>
        where TStoreEntity : StoreEntity, IDataStoreEntity
        where TDatabase : DbContext
    {
        protected DataStoreEntityRepositoryBase(
            RepositoryCacheSettings repositoryCacheSettings,
            IRepositoryCacheService<TStoreEntity> repositoryCacheService, IDatabaseAccessRepositoryFactory<TDatabase> databaseAccessRepositoryFactory) : base(repositoryCacheSettings, repositoryCacheService, databaseAccessRepositoryFactory)
        {

        }

        public async Task<TStoreEntity> ReadByIdAsync(int id, IList<Expression<Func<TStoreEntity, object>>> include = null, bool? ignoreQueryFilters = false)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            return await UseSingleItemCacheServiceIfRequired(BuildKey(nameof(ReadByIdAsync), id, include is not null ? string.Join(";", include.Select(i => i.ToString())) : "$"), async () =>
            {
                return await UseStoreRepositoryAndReturnAsync(async (db, set) =>
                {
                    var q = BuildQueryable(set, include);

                    if (ignoreQueryFilters.HasValue && ignoreQueryFilters.Value)
                        q = q.IgnoreQueryFilters();

                    var entity = await q.FirstOrDefaultAsync(e => e.Id == id);

                    if (Equals(null, entity)) throw new EntityNotFoundException(id.ToString());

                    return entity;

                }, false);
            });
        }

        public async Task<TStoreEntity> ReadSingleAsync(Expression<Func<TStoreEntity, bool>> lambda, IList<Expression<Func<TStoreEntity, object>>> include = null, bool? ignoreQueryFilters = false, bool throwIfDoesNotExist = false)
        {
            Guard.NotNull(lambda);

            return await UseSingleItemCacheServiceIfRequired(BuildReadDataKey(nameof(ReadSingleAsync), lambda, default, default, default, include, false, false, false), async () =>
            {
                return await UseStoreRepositoryAndReturnAsync(async (db, set) =>
                {
                    var q = BuildQueryable(set, include);

                    if (ignoreQueryFilters.HasValue && ignoreQueryFilters.Value)
                        q = q.IgnoreQueryFilters();

                    var entity = await q.FirstOrDefaultAsync(lambda);

                    if (throwIfDoesNotExist && Equals(null, entity)) throw new EntityNotFoundException();

                    return entity;

                }, false);
            });
        }

        public async Task<PaginatedResult<TStoreEntity>> ReadAsync(
            Expression<Func<TStoreEntity, bool>> lambda,
            (int skip, int take) paging,
            Expression<Func<TStoreEntity, object>>? orderBy = null,
            Expression<Func<TStoreEntity, object>>? orderByDescending = null,
            IList<Expression<Func<TStoreEntity, object>>>? include = null,
            bool asNoTracking = false,
            bool? checkSingle = null,
            bool? ignoreQueryFilters = false,
            bool? refuseTotal = false)
            => await ReadAsync(
                    lambda,
                    c => c,
                    paging,
                    orderBy,
                    orderByDescending,
                    include,
                    asNoTracking,
                    checkSingle,
                    ignoreQueryFilters
                    );

        public async Task<PaginatedResult<TProjectedEntity>> ReadAsync<TProjectedEntity>(
            Expression<Func<TStoreEntity, bool>> lambda,
            Expression<Func<TStoreEntity, TProjectedEntity>> projection,
            (int skip, int take) paging,
            Expression<Func<TStoreEntity, object>>? orderBy = null,
            Expression<Func<TStoreEntity, object>>? orderByDescending = null,
            IList<Expression<Func<TStoreEntity, object>>>? include = null,
            bool asNoTracking = false,
            bool? checkSingle = null,
            bool? ignoreQueryFilters = false,
            bool? refuseTotal = false)
        {
            return await UsePaginatedProjectedCacheServiceIfRequired(BuildReadDataKey(nameof(ReadAsync), lambda, default, orderBy, orderByDescending, include, asNoTracking, checkSingle, refuseTotal),
                async () =>
            {
                return await UseStoreRepositoryAndReturnAsync(async (db, set) =>
                {
                    var totalCount = refuseTotal.HasValue && refuseTotal.Value ? -1 : await BuildQueryable(set, default!).Where(lambda).CountAsync();

                    var q = await ReadDataInternal(
                        db,
                        set,
                        lambda,
                        orderBy,
                        orderByDescending,
                        paging,
                        default,
                        include,
                        asNoTracking,
                        checkSingle,
                        ignoreQueryFilters);

                    var pq = q.Select(projection);

                    var data = await ExecuteQueryInternal(pq, checkSingle);

                    return new PaginatedResult<TProjectedEntity>(totalCount, data);

                }, false);
            });
        }

        public async Task<IList<TStoreEntity>> ReadAsync(
            Expression<Func<TStoreEntity, bool>> lambda,
            Expression<Func<TStoreEntity, object>>? orderBy = null,
            Expression<Func<TStoreEntity, object>>? orderByDescending = null,
            IList<Expression<Func<TStoreEntity, object>>>? include = null,
            int? take = default,
            bool asNoTracking = false,
            bool? checkSingle = null,
            bool? ignoreQueryFilters = false)
                => await ReadAsync(
                    lambda,
                    c => c,
                    orderBy,
                    orderByDescending,
                    include,
                    take,
                    asNoTracking,
                    checkSingle,
                    ignoreQueryFilters
                    );

        public async Task<IList<TProjectedEntity>> ReadAsync<TProjectedEntity>(
            Expression<Func<TStoreEntity, bool>> lambda,
            Expression<Func<TStoreEntity, TProjectedEntity>> projection,
            Expression<Func<TStoreEntity, object>>? orderBy = null,
            Expression<Func<TStoreEntity, object>>? orderByDescending = null,
            IList<Expression<Func<TStoreEntity, object>>>? include = null,
            int? take = default,
            bool asNoTracking = false,
            bool? checkSingle = null,
            bool? ignoreQueryFilters = false)
        {
            Guard.NotNull(lambda, nameof(lambda));

            return await UseProjectedItemCollectionCacheServiceIfRequired(BuildReadDataKey(nameof(ReadAsync), lambda, default, orderBy, orderByDescending, include, asNoTracking, checkSingle, false), async () =>
            {
                return await UseStoreRepositoryAndReturnAsync(async (db, set) =>
                {
                    var q = await ReadDataInternal(
                        db,
                        set,
                        lambda,
                        orderBy,
                        orderByDescending,
                        default,
                        take,
                        include,
                        asNoTracking,
                        checkSingle,
                        ignoreQueryFilters);

                    var pq = q.Select(projection);

                    return await ExecuteQueryInternal(pq, checkSingle);

                }, false);
            });
        }

        public async Task<IList<TStoreEntity>> ReadAllAsync(IList<Expression<Func<TStoreEntity, object>>> include = null, bool? ignoreQueryFilters = false)
        {
            return await UseItemCollectionCacheServiceIfRequired(BuildKey(nameof(ReadAllAsync), include is not null ? string.Join(";", include.Select(i => i.ToString())) : "$"), async () =>
            {
                return await UseStoreRepositoryAndReturnAsync(async (db, set) =>
                {
                    var q = BuildQueryable(set, include);

                    if (ignoreQueryFilters.HasValue && ignoreQueryFilters.Value)
                    {
                        q = q.IgnoreQueryFilters();
                    }

                    var entities = await q.ToListAsync();

                    return entities;

                }, false);
            });
        }

        public async Task<IList<TStoreEntity>> ReadSqlAsync(string sql)
        {
            return await UseItemCollectionCacheServiceIfRequired(BuildKey(nameof(ReadSqlAsync), sql), async () =>
            {
                return await UseStoreRepositoryAndReturnAsync(async (db, set) =>
                {
                    return await set.FromSqlRaw(sql).ToListAsync();

                }, false);
            });
        }

        public async Task<TStoreEntity> CreateAsync(TStoreEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            return await UseStoreRepositoryAndReturnAsync(async (db, set) =>
            {
                await set.AddAsync(entity);

                return entity;
            },
            true,
            onAfterUpdate: async () => await ItemCacheServiceOnCreate(entity));
        }

        public async Task CreateAsync(IList<TStoreEntity> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            if (entities.Count == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(entities));

            await UseStoreRepositoryAsync(async (db, set) =>
            {
                await set.AddRangeAsync(entities);

            },
            true,
            onAfterUpdate: async () => { foreach (var entity in entities) await ItemCacheServiceOnCreate(entity); });
        }

        public async Task<TStoreEntity> UpdateAsync(TStoreEntity entity, params Func<TStoreEntity, object>[] referencialEntitiesToUpdate)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            return await UseStoreRepositoryAndReturnAsync(async (db, set) =>
            {
                db.Entry(entity).State = EntityState.Modified;

                if (referencialEntitiesToUpdate is not null and { Length: > 0 })
                {
                    foreach (var reference in referencialEntitiesToUpdate.Where(r => r(entity) is not null))
                        db.Entry(reference(entity)).State = EntityState.Modified;
                }

                await Task.CompletedTask;

                return entity;

            }, true,
            onAfterUpdate: async () => await ItemCacheServiceOnModify(entity));
        }

        public async Task<TStoreEntity> UpdateAsync(int entityId, Action<TStoreEntity> entityUpdater)
        {
            Guard.NumberMin(entityId, 1);
            Guard.NotNull(entityUpdater);

            TStoreEntity entity = default;

            return await UseStoreRepositoryAndReturnAsync(async (db, set) =>
            {
                entity = await set.FindAsync(entityId);

                if (Equals(null, entity)) throw new EntityNotFoundException(entityId.ToString());

                entityUpdater(entity);

                return entity;

            }, true,
            onAfterUpdate: async () => await ItemCacheServiceOnModify(entity));
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            TStoreEntity entity = default;

            await UseStoreRepositoryAsync(async (db, set) =>
            {
                entity = await set.FindAsync(id);

                if (Equals(null, entity)) throw new EntityNotFoundException(id.ToString());

                set.Remove(entity);

            },
            true,
            onAfterUpdate: async () => await ItemCacheServiceOnDelete(entity));
        }

        public async Task DeleteAsync(int id, IList<Expression<Func<TStoreEntity, object>>> include = null)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            TStoreEntity entity = default;

            await UseStoreRepositoryAsync(async (db, set) =>
            {
                var q = BuildQueryable(set, include);

                var entity = await q.FirstAsync(x => x.Id == id);

                if (Equals(null, entity)) throw new EntityNotFoundException(id.ToString());

                set.Remove(entity);

            },
            true,
            onAfterUpdate: async () => await ItemCacheServiceOnDelete(entity));
        }

        private async Task<IQueryable<TStoreEntity>> ReadDataInternal(
            TDatabase database,
            DbSet<TStoreEntity> set,
            Expression<Func<TStoreEntity, bool>> lambda,
            Expression<Func<TStoreEntity, object>>? orderBy = null,
            Expression<Func<TStoreEntity, object>>? orderByDescending = null,
            (int skip, int take)? paging = default,
            int? take = default,
            IList<Expression<Func<TStoreEntity, object>>>? include = null,
            bool asNoTracking = false,
            bool? checkSingle = null,
            bool? ignoreQueryFilters = false)
        {
            var q = BuildQueryable(set, include);

            var entitiesQuery = q.Where(lambda);

            if (asNoTracking) entitiesQuery = entitiesQuery.AsNoTracking();

            if (orderBy is not null)
            {
                entitiesQuery = entitiesQuery.OrderBy(orderBy);
            }

            if (orderByDescending is not null)
            {
                entitiesQuery = entitiesQuery.OrderByDescending(orderByDescending);
            }

            if (ignoreQueryFilters.HasValue && ignoreQueryFilters.Value)
            {
                entitiesQuery = entitiesQuery.IgnoreQueryFilters();
            }

            if (paging is not null)
            {
                entitiesQuery = entitiesQuery
                .Skip(paging.Value.skip)
                .Take(paging.Value.take);
            }

            if (take.HasValue)
            {
                entitiesQuery = entitiesQuery
                .Take(take.Value);
            }

            return entitiesQuery;
        }

        private async Task<IList<TReturn>> ExecuteQueryInternal<TReturn>(
            IQueryable<TReturn> q,
            bool? checkSingle = null)
        {
            var entities = await q.ToListAsync();

            if (Equals(null, checkSingle) || !checkSingle.Value) return entities;

            if (entities.Count > 1) throw new InvalidOperationException("Multiple entities found");

            if (entities.Count == 0) throw new InvalidOperationException("No entities found");

            return entities;
        }

        private CacheKey BuildReadDataKey(
            string methodName,
            Expression<Func<TStoreEntity, bool>> lambda,
            (int skip, int take)? paging,
            Expression<Func<TStoreEntity, object>>? orderBy,
            Expression<Func<TStoreEntity, object>>? orderByDescending,
            IList<Expression<Func<TStoreEntity, object>>>? include,
            bool asNoTracking,
            bool? checkSingle,
            bool? refuseTotal)
        {
            return BuildKey(
                                methodName,
                                ExpressionEvaluator.PartialEval(lambda).ToString().ToMd5Fingerprint(),
                                paging.HasValue ? $"PS{paging.Value.skip};PT{paging.Value.take}" : "$",
                                orderBy is not null ? ExpressionEvaluator.PartialEval(orderBy).ToString().ToMd5Fingerprint() : "$",
                                orderByDescending is not null ? ExpressionEvaluator.PartialEval(orderByDescending).ToString().ToMd5Fingerprint() : "$",
                                include is not null ? string.Join(";", include.Select(i => i.ToString())) : "$",
                                asNoTracking,
                                checkSingle.HasValue ? checkSingle.Value : "$",
                                refuseTotal.HasValue ? refuseTotal.Value : "$"
                            );
        }
    }
}
