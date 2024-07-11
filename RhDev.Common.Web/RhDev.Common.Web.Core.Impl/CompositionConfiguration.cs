using Lamar;
using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.DataAccess.Caching;
using RhDev.Common.Web.Core.DataAccess.Concurrent;
using RhDev.Common.Web.Core.Impl.Caching;
using RhDev.Common.Web.Core.Impl.Threading;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Web.Core.Impl
{
    public class CompositionConfiguration : ConventionConfigurationBase
    {
        public CompositionConfiguration(ServiceRegistry configuration, Container container) : base(configuration, container)
        {
        }

        public override void Apply()
        {
            base.Apply();

            ConfigureCache();
        }

        private void ConfigureCache()
        {
            For(typeof(IRepositoryCacheService<>)).Use(typeof(RepositoryCacheService<>));

            ConfigureAsSingleton(typeof(ISynchronousCache<>), typeof(SynchronousDictionaryCache<>));
            ConfigureAsSingleton(typeof(IAsynchronousCache<>), typeof(AsynchronousExpirationDictionaryCache<>));
            
            ConfigureAsSingleton(typeof(IConcurrentDataAccessRepository), typeof(ConcurrentDataAccessRepository));
                        
            For<BuildInfo>().Use<BuildInfo>().Singleton();            
        }
    }
}
