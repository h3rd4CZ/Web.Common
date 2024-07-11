using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.DataAccess.SQL;
using System.Linq.Expressions;

namespace RhDev.Common.Web.Core.DataAccess.DataSync
{
    public interface IRemoteDataComparer<TStoreEntity, TRemoteDataEntity> : IAutoregisteredService where TStoreEntity : IDataStoreEntity where TRemoteDataEntity : class
    {
        public Task<bool> SyncDataEntity(
            TStoreEntity storeEntity,
            TRemoteDataEntity input,
            IList<(
                Expression<Func<TStoreEntity, object>> storeEntityPropertyEvaluator,
                Func<TRemoteDataEntity, object> remoteEntityPropertyEvaluator,
                Func<TStoreEntity, TRemoteDataEntity, Task> changeAction)> comparables);
    }
}
