using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Stores;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.DataAccess.Sql.Repository.Stores
{
    public class DynamicDataStoreRepository<TContext> : IDynamicDataStoreRepository<TContext> where TContext : DbContext
    {
        private readonly IDatabaseAccessRepositoryFactory<TContext> factory;

        public DynamicDataStoreRepository(IDatabaseAccessRepositoryFactory<TContext> factory)
        {
            this.factory = factory;
        }

        public async Task<TEntityShape> UpdateEntityAsync<TEntityShape>(TEntityShape entityShape) where TEntityShape : class, IDataStoreEntity
        {
            Guard.NotNull(entityShape);

            await factory.RunActionAsync(async (db) =>
            {
                var entity = db.Entry(entityShape);

                entity.State = EntityState.Modified;

                await db.SaveChangesAsync();
            });

            return entityShape;
        }

        public async Task<TEntityShape> WriteEntityAsync<TEntityShape>(TEntityShape entity) where TEntityShape : class, IDataStoreEntity
        {
            Guard.NotNull(entity);

            await factory.RunActionAsync(async (db) =>
            {
                var set = db.Set<TEntityShape>();

                await set.AddAsync(entity);

                await db.SaveChangesAsync();

                Guard.NotNull(set);
            });

            return entity;
        }

        public async Task<TEntityShape> ReadEntityByIdAsync<TEntityShape>(string entityTypeName, int entityId, string[]? navigationPropertiesInclude = default) where TEntityShape : class, IDataStoreEntity
        {
            Guard.StringNotNullOrWhiteSpace(entityTypeName);
            Guard.NumberMin(entityId, 1);

            var entityType = Type.GetType(entityTypeName, false);

            Guard.NotNull(entityTypeName, nameof(entityType), $"Type was not found : {entityType}");

            return await ReadEntityByIdAsync<TEntityShape>(entityType!, entityId, navigationPropertiesInclude);
        }

        public async Task<TEntityShape> ReadEntityByIdAsync<TEntityShape>(Type entityType, int id, string[]? navigationPropertiesInclude = default) where TEntityShape : class, IDataStoreEntity
        {
            Guard.NotNull(entityType);
            Guard.NumberMin(id, 1);

            TEntityShape entity = default;

            await factory.RunActionAsync(async (db) =>
            {
                var set = GetDbSetByType(db, entityType);

                Guard.NotNull(set);

                var q = set.OfType<TEntityShape>();

                if (navigationPropertiesInclude is not null and { Length: > 0 })
                {
                    foreach (var include in navigationPropertiesInclude) q = q.Include(include);
                }

                entity = await q.FirstOrDefaultAsync(e => e.Id == id);
            });

            return entity;
        }

        public async Task<TEntityShape?> ReadEntityByLambdaAsync<TEntityShape>(Type entityType, Expression<Func<TEntityShape, bool>> lambda) where TEntityShape : class
        {
            Guard.NotNull(entityType);
            Guard.NotNull(lambda);

            TEntityShape? entity = default;

            await factory.RunActionAsync(async (db) =>
            {
                var set = GetDbSetByType(db, entityType);

                Guard.NotNull(set);

                entity = await set.OfType<TEntityShape>()
                .FirstOrDefaultAsync(lambda);
            });

            return entity;
        }

        IQueryable GetDbSetByType(TContext dbContext, Type entityType)
        {
            // Use reflection to find the DbSet property by type
            var dbSetProperty = dbContext.GetType().GetProperties()
                .FirstOrDefault(p => p.PropertyType.IsGenericType &&
                                     p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                                     (p.PropertyType.GetGenericArguments().First() == entityType || entityType.IsAssignableFrom(p.PropertyType.GetGenericArguments().First())));


            Guard.NotNull(dbSetProperty, nameof(dbSetProperty), message: $"No Db Set with type name : {entityType.Name} was found on context");

            return (IQueryable)dbSetProperty.GetValue(dbContext);

        }

        public async Task<IList<TEntityShape>> ReadAllEntitiesByLambda<TEntityShape>(Func<TEntityShape, bool> fuction) where TEntityShape : class, IDataStoreEntity
        {
            IList<TEntityShape> entity = default;

            await factory.RunActionAsync(async (db) =>
            {
                var set = GetDbSetByType(db, typeof(TEntityShape));

                Guard.NotNull(set);

                entity = set.OfType<TEntityShape>().Where(fuction).ToList();
            });

            return entity;
        }

        public async Task<bool> DeleteEntityByIdAsync<TEntityShape>(Type entityType, int id) where TEntityShape : class, IDataStoreEntity
        {
            Guard.NotNull(entityType);
            Guard.NumberMin(id, 1);

            await factory.RunActionAsync(async (db) =>
            {
                var set = GetDbSetByType(db, entityType);

                var entity = await set.OfType<TEntityShape>().FirstOrDefaultAsync(x => x.Id == id);

                Guard.NotNull(entity);

                db.Remove(entity);

                await db.SaveChangesAsync();
            });

            return true;
        }

        public async Task<bool> EntityExistsByLambda<TEntityShape>(Func<TEntityShape, bool> fuction) where TEntityShape : class, IDataStoreEntity
        {
            bool result = false;

            await factory.RunActionAsync(async (db) =>
            {
                var set = GetDbSetByType(db, typeof(TEntityShape));

                Guard.NotNull(set);

                result = set.OfType<TEntityShape>().Any(fuction);
            });

            return result;
        }
    }
}
