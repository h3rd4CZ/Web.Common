using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.Composition;

namespace RhDev.Common.Web.Core.Counters
{
    public interface ICounterSettingsManager<TCounter> : IAutoregisteredService where TCounter : class
    {
        Task WriteCounterAsync(TCounter counter, string userId, RepositoryCacheStrategy repositoryCacheStrategy = RepositoryCacheStrategy.AlwaysBypass);
        public Task<TCounter> ReadCounterAsync();
    }
}
