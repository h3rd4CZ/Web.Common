using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.DataAccess.SQL.Repository.Stores
{
    public interface IDynamicDataStoreRepository<TContext> : IAutoregisteredService where TContext : DbContext
    {
        Task<TEntityShape> UpdateEntityAsync<TEntityShape>(TEntityShape entityShape) where TEntityShape : class, IDataStoreEntity;
        Task<TEntityShape> WriteEntityAsync<TEntityShape>(TEntityShape entity) where TEntityShape : class, IDataStoreEntity;
        Task<TEntityShape> ReadEntityByIdAsync<TEntityShape>(string entityTypeName, int entityId, string[]? navigationPropertiesInclude = default) where TEntityShape : class, IDataStoreEntity;
        Task<TEntityShape> ReadEntityByIdAsync<TEntityShape>(Type entityType, int id, string[]? navigationPropertiesInclude = default) where TEntityShape : class, IDataStoreEntity;
        Task<TEntityShape?> ReadEntityByLambdaAsync<TEntityShape>(Type entityType, Expression<Func<TEntityShape, bool>> lambda) where TEntityShape : class;
        Task<IList<TEntityShape>> ReadAllEntitiesByLambda<TEntityShape>(Func<TEntityShape, bool> fuction) where TEntityShape : class, IDataStoreEntity;
        Task<bool> DeleteEntityByIdAsync<TEntityShape>(Type entityType, int id) where TEntityShape : class, IDataStoreEntity;
        Task<bool> EntityExistsByLambda<TEntityShape>(Func<TEntityShape, bool> fuction) where TEntityShape : class, IDataStoreEntity;
    }
}
