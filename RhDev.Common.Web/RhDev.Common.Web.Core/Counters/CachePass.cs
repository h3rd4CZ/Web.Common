using RhDev.Common.Web.Core.Caching;

namespace RhDev.Common.Web.Core.Counters
{
    public class CachePass : IDisposable
    {
        [ThreadStatic]
        private static RepositoryCacheStrategy cacheStrategy = RepositoryCacheStrategy.AlwaysBypass;

        public CachePass(RepositoryCacheStrategy repositoryCacheStrategy = RepositoryCacheStrategy.ReadOnly)
        {
            cacheStrategy = repositoryCacheStrategy;
        }

        public static RepositoryCacheStrategy CacheStrategy => cacheStrategy;

        public void Dispose()
        {
            cacheStrategy = RepositoryCacheStrategy.AlwaysBypass;
        }
    }
}
