using Microsoft.Extensions.Logging;
using RhDev.Common.Web.Core.Caching;

namespace RhDev.Common.Web.Core.Impl.Caching
{
    public class SynchronousDictionaryCache<TValue> : ISynchronousCache<TValue> where TValue : class
    {
        private readonly IDictionary<CacheKey, TValue> cache = new Dictionary<CacheKey, TValue>();
        private readonly ILogger<SynchronousDictionaryCache<TValue>> traceLogger;

        public IList<KeyValuePair<CacheKey, TValue>> GetCacheContent
        {
            get
            {
                lock (cache)
                {
                    return cache.AsEnumerable().ToList();
                }
            }
        }
        public SynchronousDictionaryCache(ILogger<SynchronousDictionaryCache<TValue>> traceLogger)
        {
            this.traceLogger = traceLogger;
        }

        public void AddValue(CacheKey key, TValue value)
        {
            
            if (key == null)
                throw new ArgumentNullException("key");

            if (value == null)
                throw new ArgumentNullException("value");

            lock (cache)
                cache[key] = value;
        }

        public TValue GetValue(CacheKey key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            lock (cache)
            {
                TValue value;
                cache.TryGetValue(key, out value);

                return value;
            }
        }

        public TValue GetOrFetchValue(CacheKey key, Func<TValue> valueProvider)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (valueProvider == null)
                throw new ArgumentNullException("valueProvider");

            if (CacheBypass.IsActive)
            {
                return valueProvider();
            }

            lock (cache)
            {
                TValue value = GetValue(key) ?? FetchValue(key, valueProvider);
                return value;
            }
        }

        private TValue FetchValue(CacheKey key, Func<TValue> valueProvider)
        {
            traceLogger.LogTrace("Cache of {0}: miss for key '{1}', fetching value", typeof (TValue), key);
            TValue value = valueProvider();

            if (value != null)
                AddValue(key, value);
            else
                traceLogger.LogTrace("Cache of {0}: null value fetched", typeof (TValue));

            return value;
        }

        public void Clear()
        {
            lock (cache)
            {
                traceLogger.LogTrace("Cache of {0}: clearing requested", typeof(TValue));
                cache.Clear();
            }
        }

        public void InvalidateCacheItem(CacheKey key)
        {
            if (key == null) throw new ArgumentNullException("key");

            lock (cache)
            {
                cache.Remove(key);
            }
        }
    }
}
