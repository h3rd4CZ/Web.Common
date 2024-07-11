using RhDev.Common.Web.Core.Composition;
using System;
using System.Collections.Generic;

namespace RhDev.Common.Web.Core.Caching
{
    public interface ISynchronousCache<TValue> : IService where TValue : class
    {
        IList<KeyValuePair<CacheKey, TValue>> GetCacheContent { get; }

        void AddValue(CacheKey key, TValue value);

        TValue GetValue(CacheKey key);

        TValue GetOrFetchValue(CacheKey key, Func<TValue> valueProvider);

        void Clear();
        void InvalidateCacheItem(CacheKey key);
    }
}
