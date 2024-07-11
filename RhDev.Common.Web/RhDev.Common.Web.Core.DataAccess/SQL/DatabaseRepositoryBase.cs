using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.DataAccess.Caching;
using System.Linq.Expressions;

namespace RhDev.Common.Web.Core.DataAccess.SQL
{
    public abstract class DatabaseRepositoryBase<TStoreEntity, TDatabase> : RepositoryCacheServiceBase<TStoreEntity>
        where TStoreEntity : StoreEntity, IDataStoreEntity
        where TDatabase : DbContext
    {
        private const string COMMON_ENTITY_IDENTIFIER = "Id";

        public IDatabaseAccessRepositoryFactory<TDatabase> databaseAccessRepositoryFactory { get; set; }
        protected DatabaseRepositoryBase(
            RepositoryCacheSettings repositoryCacheSettings,
            IRepositoryCacheService<TStoreEntity> repositoryCacheService,
            IDatabaseAccessRepositoryFactory<TDatabase> databaseAccessRepositoryFactory) : base(repositoryCacheSettings, repositoryCacheService)
        {
            this.databaseAccessRepositoryFactory = databaseAccessRepositoryFactory;
        }

        protected async Task UseStoreRepositoryAsync(
            Func<TDatabase, DbSet<TStoreEntity>, Task> a,
            bool? save = null,
            TDatabase database = null,
            Func<Task> onAfterUpdate = default)
        {
            if (!Equals(null, database))
                await UseDatabaseAsync(a, database, save, onAfterUpdate);

            else
            {
                await databaseAccessRepositoryFactory.RunActionAsync(async db =>
                {
                    await UseDatabaseAsync(a, db, save, onAfterUpdate);
                });
            }
        }

        protected async Task UseDatabaseAsync(
            Func<TDatabase, DbSet<TStoreEntity>, Task> a,
            TDatabase database,
            bool? save = null,
            Func<Task> onAfterUpdate = default)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));

            var set = database.Set<TStoreEntity>();

            await a(database, set);

            if (!Equals(null, save) && save.Value) await database.SaveChangesAsync();

            if (onAfterUpdate is not null) await onAfterUpdate();
        }

        protected async Task<TReturn> UseDatabaseAndReturnAsync<TReturn>(
            Func<TDatabase, DbSet<TStoreEntity>, Task<TReturn>> a,
            TDatabase database,
            bool? save = null,
            Func<Task> onAfterUpdate = default)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            var set = database.Set<TStoreEntity>();

            TReturn ret = await a(database, set);

            if (!Equals(null, save) && save.Value) await database.SaveChangesAsync();

            if (onAfterUpdate is not null) await onAfterUpdate();

            return ret;
        }

        protected async Task<TReturn> UseStoreRepositoryAndReturnAsync<TReturn>(
            Func<TDatabase, DbSet<TStoreEntity>, Task<TReturn>> a,
            bool? save = null,
            TDatabase database = null,
            Func<Task> onAfterUpdate = default)
        {
            TReturn ret = default;

            if (!Equals(null, database))
                ret = await UseDatabaseAndReturnAsync(a, database, save, onAfterUpdate);
            else
            {
                await databaseAccessRepositoryFactory.RunActionAsync(async db =>
                {
                    ret = await UseDatabaseAndReturnAsync(a, db, save, onAfterUpdate);
                });
            }

            return ret;
        }

        protected void LoadRelatedIfAny<TCollection>(
            TDatabase db,
            TStoreEntity entity,
            Expression<Func<TStoreEntity, IEnumerable<TCollection>>> relatedCollection,
            Action<IQueryable<TCollection>> relatedExpression) where TCollection : class
        {
            if (Equals(null, entity)) return;

            if (Equals(null, relatedCollection)) return;

            if (Equals(null, relatedExpression))
                throw new InvalidOperationException(
                    "If Related collection is not null related expression must be defined");

            var query = db.Entry(entity)
                .Collection(relatedCollection)
                .Query();

            relatedExpression(query);
        }

        protected async Task LoadNavigationIfAnyAsync<TNavigation>(
            TDatabase db,
            TStoreEntity entity,
            Expression<Func<TStoreEntity, TNavigation>> navigationResolver) where TNavigation : class
        {
            if (Equals(null, entity)) return;

            if (Equals(null, navigationResolver)) return;

            await db.Entry(entity)
                .Reference(navigationResolver)
                .LoadAsync();
        }

        protected IQueryable<TStoreEntity> BuildQueryable(DbSet<TStoreEntity> set, IList<Expression<Func<TStoreEntity, object>>> include)
        {
            IQueryable<TStoreEntity> q = !Equals(null, include) && include.Any() ? set.Include(include.First()) : default(IQueryable<TStoreEntity>);

            if (!Equals(null, include))
            {
                for (var i = 1; i < include.Count; i++)
                    q = q.Include(include[i]);
            }
            else
            {
                q = set;
            }

            return q;
        }

        protected Func<TStoreEntity, bool> BuildGetByIdLambda(int id)
        {
            var param = Expression.Parameter(typeof(TStoreEntity));
            var property = Expression.Property(param, typeof(TStoreEntity), COMMON_ENTITY_IDENTIFIER);

            var exprId = Expression.Constant(id);

            var getByIdCall = Expression.Equal(property, exprId);

            return Expression.Lambda<Func<TStoreEntity, bool>>(getByIdCall, param).Compile();

        }
    }

}
