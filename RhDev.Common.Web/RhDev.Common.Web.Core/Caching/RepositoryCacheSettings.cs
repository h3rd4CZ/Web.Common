namespace RhDev.Common.Web.Core.Caching
{
    public class RepositoryCacheSettings
    {
        public RepositoryCacheStrategy CacheStrategy { get; }

        public RepositoryCacheSettings(RepositoryCacheStrategy cacheStrategy)
        {
            CacheStrategy = cacheStrategy;
        }

        public static RepositoryCacheSettings ForCacheStrategy(RepositoryCacheStrategy cacheStrategy) => new RepositoryCacheSettings(cacheStrategy);
    }
}
