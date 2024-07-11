using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.DataStore;
using System.Linq.Expressions;

namespace RhDev.Common.Web.Core.DataAccess
{
    public interface IStoreRepository<TStoreEntity> : IDataStoreRepository
        where TStoreEntity : IDataStoreEntity

    {
        /// <summary>
        /// Read by id with lazy loading of related entities
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TStoreEntity> ReadByIdAsync(
            int id,
            IList<Expression<Func<TStoreEntity, object>>> include = null,
            bool? ignoreQueryFilter = false);


        /// <summary>
        /// Reads single item using lambda
        /// </summary>
        /// <param name="lambda"></param>
        /// <param name="include"></param>
        /// <param name="ignoreQueryFilters"></param>
        /// <param name="throwIfDoesNotExist"></param>
        /// <returns></returns>
        Task<TStoreEntity> ReadSingleAsync(
            Expression<Func<TStoreEntity, bool>> lambda,
            IList<Expression<Func<TStoreEntity, object>>> include = null,
            bool? ignoreQueryFilters = false,
            bool throwIfDoesNotExist = false);

        /// <summary>
        /// /// Works as same as ReadAsync but with Paginated result
        /// </summary>
        /// <param name="lambda"> lambda expression to retrieve data</param>
        /// <param name="orderBy">order by expression</param>
        /// <param name="orderByDescending">order by descending expression</param>
        /// <param name="paging">paging information</param>
        /// <param name="include">include related data as eager loaded data</param>
        /// <param name="asNoTracking">Data wont be tracked anymore</param>
        /// <param name="checkSingle">Check if result set contains only one item</param>
        /// <param name="ignoreQueryFilter">Ignores globally configured filters</param>
        /// <param name="refuseTotal">Does not count total items with extra query</param>A
        /// <returns></returns>
        Task<PaginatedResult<TStoreEntity>> ReadAsync(
            Expression<Func<TStoreEntity, bool>> lambda,
            (int skip, int take) paging,
            Expression<Func<TStoreEntity, object>>? orderBy = null,
            Expression<Func<TStoreEntity, object>>? orderByDescending = null,
            IList<Expression<Func<TStoreEntity, object>>>? include = null,
            bool asNoTracking = false,
            bool? checkSingle = null,
            bool? ignoreQueryFilter = false, 
            bool? refuseTotal = false);

        /// <summary>
        /// Same as ReadPaginated but with projection option
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="lambda"></param>
        /// <param name="projection"></param>
        /// <param name="paging"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderByDescending"></param>
        /// <param name="include"></param>
        /// <param name="asNoTracking"></param>
        /// <param name="checkSingle"></param>
        /// <param name="ignoreQueryFilter"></param>
        /// <param name="refuseTotal">Does not count total items with extra query</param>A
        /// <returns></returns>
        Task<PaginatedResult<TReturn>> ReadAsync<TReturn>(
            Expression<Func<TStoreEntity, bool>> lambda,
            Expression<Func<TStoreEntity, TReturn>> projection,
            (int skip, int take) paging,
            Expression<Func<TStoreEntity, object>>? orderBy = null,
            Expression<Func<TStoreEntity, object>>? orderByDescending = null,
            IList<Expression<Func<TStoreEntity, object>>>? include = null,
            bool asNoTracking = false,
            bool? checkSingle = null,
            bool? ignoreQueryFilter = false,
            bool? refuseTotal = false);

        /// <summary>
        /// Read data using lambda expression as query, with optional ordering, paging, including
        /// </summary>
        /// <param name="lambda"> lambda expression to retrieve data</param>
        /// <param name="orderBy">order by expression</param>
        /// <param name="orderByDescending">order by descending expression</param>
        /// <param name="paging">paging information</param>
        /// <param name="include">include related data as eager loaded data</param>
        /// <param name="asNoTracking">Data wont be tracked anymore</param>
        /// <param name="checkSingle">Check if result set contains only one item</param>
        /// <param name="ignoreQueryFilter">Ignores globally configured filters</param>
        Task<IList<TStoreEntity>> ReadAsync(
            Expression<Func<TStoreEntity, bool>> lambda,
            Expression<Func<TStoreEntity, object>>? orderBy = null,
            Expression<Func<TStoreEntity, object>>? orderByDescending = null,
            IList<Expression<Func<TStoreEntity, object>>>? include = null,
            int? take = default,
            bool asNoTracking = false,
            bool? checkSingle = null,
            bool? ignoreQueryFilter = false);

        /// <summary>
        /// As ReadAsync with projection, without caching
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="lambda"></param>
        /// <param name="projection"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderByDescending"></param>
        /// <param name="include"></param>
        /// <param name="asNoTracking"></param>
        /// <param name="checkSingle"></param>
        /// <param name="ignoreQueryFilters"></param>
        /// <returns></returns>
        Task<IList<TEntity>> ReadAsync<TEntity>(
            Expression<Func<TStoreEntity, bool>> lambda,
            Expression<Func<TStoreEntity, TEntity>> projection,
            Expression<Func<TStoreEntity, object>>? orderBy = null,
            Expression<Func<TStoreEntity, object>>? orderByDescending = null,
            IList<Expression<Func<TStoreEntity, object>>>? include = null,
            int? take = default,
            bool asNoTracking = false,
            bool? checkSingle = null,
            bool? ignoreQueryFilters = false);
        

            /// <summary>
            /// Read by expression include related entities in one query if include is not null
            /// </summary>
            /// <returns></returns>
            Task<IList<TStoreEntity>> ReadAllAsync(
            IList<Expression<Func<TStoreEntity, object>>> include = null,
            bool? ignoreQueryFilter = false);

        Task<TStoreEntity> CreateAsync(TStoreEntity entity);
        Task CreateAsync(IList<TStoreEntity> entities);
        Task<TStoreEntity> UpdateAsync(TStoreEntity entity, params Func<TStoreEntity, object>[] referencialEntitiesToUpdate);
        /// <summary>
        /// Updates entity in same DB context object. For Entity tracking scenarios
        /// </summary>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="entityUpdater">Updater</param>
        /// <returns></returns>
        Task<TStoreEntity> UpdateAsync(int entityId, Action<TStoreEntity> entityUpdater);
        Task DeleteAsync(int id);
        Task DeleteAsync(int id, IList<Expression<Func<TStoreEntity, object>>> include = null);
        Task<IList<TStoreEntity>> ReadSqlAsync(string sql);
    }
}
