using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.Configuration;

namespace RhDev.Common.Web.Core.Impl.Caching
{
    public class AsynchronousExpirationDictionaryCache<TValue> : IAsynchronousCache<TValue>
    {
        private readonly IOptions<CommonConfiguration> configuration;
        private readonly ICentralClockProvider _centralCLockProvider;
        private readonly ILogger<AsynchronousExpirationDictionaryCache<TValue>> traceLogger;

        SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public IDictionary<CacheKey, ExpirationDictionaryCacheItem<TValue>> _cache { get; }
            = new Dictionary<CacheKey, ExpirationDictionaryCacheItem<TValue>>();

        public AsynchronousExpirationDictionaryCache(IOptions<CommonConfiguration> configuration, ICentralClockProvider centralCLockProvider, ILogger<AsynchronousExpirationDictionaryCache<TValue>> traceLogger)
        {
            this.configuration = configuration;
            _centralCLockProvider = centralCLockProvider;
            this.traceLogger = traceLogger;
        }


        public async Task<IList<KeyValuePair<CacheKey, TValue>>> GetCacheContentAsync() =>
                await UseLockAndReturn(async () =>
                        await Task.FromResult(_cache.ToList().Select(c => new KeyValuePair<CacheKey, TValue>(c.Key, c.Value.Value))
                            .ToList())
                );
                                        
        public async Task<TValue> GetOrFetchValueAsync(CacheKey key, Func<Task<TValue>> valueProvider)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var expirationDuration = configuration.Value.CacheConfiguration.ExpirationDurationInMinutes;

            return await UseLockAndReturn(async () =>
            {
                _cache.TryGetValue(key, out ExpirationDictionaryCacheItem<TValue> value);

                if (Equals(null, value) || CacheBypass.IsActive) return await FetchValue(key, valueProvider, Equals(null, value));

                var now = _centralCLockProvider.Now();

                if (value.InsertionDateTime.AddMinutes(expirationDuration) < now.ExportDateTime)
                    return await FetchValue(key, valueProvider, true);

                return value.Value;
            });
        }

        private async Task<TValue> FetchValue(CacheKey key, Func<Task<TValue>> valueProvider, bool refreshItem)
        {
            traceLogger.LogTrace("Cache of {0}: expired for key '{1}', fetching value", typeof(TValue), key);

            TValue value = await valueProvider();

            if (value != null && refreshItem)
            {
                await Task.Run(() =>
                {
                    _cache[key] = ExpirationDictionaryCacheItem<TValue>.Create(value, _centralCLockProvider.Now().ExportDateTime);
                });                
            }
            else
                traceLogger.LogTrace("Cache of {0}: null value fetched", typeof(TValue));

            return value;
        }
               
        
        public async Task ClearAsync()
        {
            await UseLock(async () =>
            {
                await Task.Run(() =>
                {
                    traceLogger.LogTrace("Cache of {0}: clearing requested", typeof(TValue));
                    _cache.Clear();
                });
            });
        }

        public async Task InvalidateCacheItemAsync(CacheKey key)
        {
            if (key == null) throw new ArgumentNullException("key");

            await UseLock(async () =>
            {
                await Task.Run(()=> _cache.Remove(key));
            });
        }

        public async Task InvalidateCacheAsync()
        {
            await UseLock(async () =>
            {
                await Task.Run(() => _cache.Clear());
            });
        }

        private async Task UseLock(Func<Task> task)
        {
            await semaphoreSlim.WaitAsync();

            try
            {
                await task();
            }
            finally { semaphoreSlim.Release(); }
        }

        private async Task<TReturn> UseLockAndReturn<TReturn>(Func<Task<TReturn>> task)
        {
            await semaphoreSlim.WaitAsync();

            try
            {
                return await task();
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
    }
}
