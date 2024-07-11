using RhDev.Common.Web.Core.DataAccess.Concurrent;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Web.Core.Impl.Threading
{
    public class ConcurrentDataAccessRepository : IConcurrentDataAccessRepository
    {
        private IDictionary<Type, object> serviceslLockStore = new Dictionary<Type, object>() { };
        private IDictionary<string, SemaphoreSlim> identifiersSemaphoreStore = new Dictionary<string, SemaphoreSlim>() { };

        public ConcurrentDataAccessRepository()
        {
            
        }
                
        public void UseService<T>(Action a) where T : ISynchronizationContextService
        {
            lock (GetLock<T>()) a();
        }

        public async Task UseIdentifierAsync(string identifier, Func<Task> f)
        {
            Guard.NotNull(identifier, nameof(identifier));

            var semaphore = GetIdentifierSemaphore(identifier);

            await semaphore.WaitAsync();

            try
            {
               await f();
            }
            finally
            {
                semaphore.Release();
            }
        }

        private SemaphoreSlim GetIdentifierSemaphore(string identifier)
        {
            EnsureIdentifierSemaphoreStoreItem(identifier);

            if (!identifiersSemaphoreStore.TryGetValue(identifier, out SemaphoreSlim semaphore)) throw new InvalidOperationException($"There is no identifier lock for identifier {identifier} in identifier lock store");

            return semaphore;
        }

        private object GetLock<T>() where T : ISynchronizationContextService
        {
            EnsureLockStoreItem<T>();

            if (!serviceslLockStore.TryGetValue(typeof(T), out object o)) throw new InvalidOperationException($"There is no lock for type {typeof(T)} in lock store");

            return o;
        }

        private void EnsureIdentifierSemaphoreStoreItem(string identifier)
        {
            lock (this)
            {
                if (!identifiersSemaphoreStore.ContainsKey(identifier)) identifiersSemaphoreStore.Add(identifier, new SemaphoreSlim(1,1));
            }
        }

        private void EnsureLockStoreItem<T>() where T : ISynchronizationContextService
        {
            lock (this)
            {
                if (!serviceslLockStore.ContainsKey(typeof(T))) serviceslLockStore.Add(typeof(T), new object());
            }
        }
    }
}
