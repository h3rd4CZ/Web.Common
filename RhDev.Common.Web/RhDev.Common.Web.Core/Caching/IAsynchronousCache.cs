using RhDev.Common.Web.Core.Composition;

namespace RhDev.Common.Web.Core.Caching
{
    public interface IAsynchronousCache<TValue> : IService
    {
        Task<IList<KeyValuePair<CacheKey, TValue>>> GetCacheContentAsync();
        Task<TValue> GetOrFetchValueAsync(CacheKey key, Func<Task<TValue>> valueProvider);
        Task InvalidateCacheAsync();
        Task InvalidateCacheItemAsync(CacheKey key);
    }
}
