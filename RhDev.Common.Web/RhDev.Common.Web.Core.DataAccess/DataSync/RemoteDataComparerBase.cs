using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.Extensions;
using RhDev.Common.Web.Core.Utils;
using System.Linq.Expressions;

namespace RhDev.Common.Web.Core.DataAccess.DataSync
{
    public abstract class RemoteDataComparerBase<TStoreEntity, TRemoteEntity>
        : IRemoteDataComparer<TStoreEntity, TRemoteEntity>
        where TStoreEntity : IDataStoreEntity
        where TRemoteEntity : class
    {
        protected readonly ICentralClockProvider centralClockProvider;

        public RemoteDataComparerBase(ICentralClockProvider centralClockProvider)
        {
            this.centralClockProvider = centralClockProvider;
        }

        public async Task<bool> SyncDataEntity(
            TStoreEntity storeEntity,
            TRemoteEntity input,
            IList<(Expression<Func<TStoreEntity, object>> storeEntityPropertyEvaluator, Func<TRemoteEntity, object> remoteEntityPropertyEvaluator, Func<TStoreEntity, TRemoteEntity, Task> changeAction)> comparables) => await Compare(storeEntity, input, comparables);

        protected virtual async Task<bool> Compare(
            TStoreEntity storeEntity,
            TRemoteEntity input,
            IList<(Expression<Func<TStoreEntity, object>> storeEntityPropertyEvaluator, Func<TRemoteEntity, object> remoteEntityPropertyEvaluator, Func<TStoreEntity, TRemoteEntity, Task> changeAction)> comparables)
        {
            bool changed = false;

            foreach (var comparer in comparables)
            {
                changed |= await CompareItemFields(storeEntity, input, comparer.storeEntityPropertyEvaluator, comparer.remoteEntityPropertyEvaluator, comparer.changeAction);
            }

            return changed;
        }

        protected async Task<bool> CompareItemFields<TProperty>(
            TStoreEntity storeEntity,
            TRemoteEntity remoteEntity,
            Expression<Func<TStoreEntity, TProperty>> storeEntityPropertyEvaluator,
            Func<TRemoteEntity, TProperty> remoteEntityPropertyEvaluator,
            Func<TStoreEntity, TRemoteEntity, Task> changeAction)
        {
            Guard.NotNull(storeEntity, nameof(storeEntity));
            Guard.NotNull(remoteEntity, nameof(remoteEntity));
            Guard.NotNull(storeEntityPropertyEvaluator, nameof(storeEntityPropertyEvaluator));
            Guard.NotNull(remoteEntityPropertyEvaluator, nameof(remoteEntityPropertyEvaluator));

            var compiledStoreEntityPropertyEvaluator = storeEntityPropertyEvaluator.Compile();

            var storeEntityPropertyValue = compiledStoreEntityPropertyEvaluator(storeEntity);
            var remoteEntityPropertyValue = remoteEntityPropertyEvaluator(remoteEntity);

            if (storeEntityPropertyValue is null)
            {
                if (remoteEntityPropertyValue is null) return false;

                if (changeAction is not null) await changeAction(storeEntity, remoteEntity);

                storeEntityPropertyEvaluator.SetProperty(storeEntity, remoteEntityPropertyValue);

                return true;
            }

            if (!storeEntityPropertyValue.Equals(remoteEntityPropertyValue))
            {
                if (changeAction is not null) await changeAction(storeEntity, remoteEntity);

                storeEntityPropertyEvaluator.SetProperty(storeEntity, remoteEntityPropertyValue);

                return true;
            }

            return false;
        }
    }
}
