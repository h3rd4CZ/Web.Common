using RhDev.Common.Web.Core.Composition;
using System.Collections.Generic;

namespace RhDev.Common.Workflow.Caching
{
    public interface IStateManagementCacheProvider : IAutoregisteredService
    {
        IList<StateManagementCacheItemRecord> ReadCachees();
        void InvalidateCacheKey(StateManagementCacheType cacheType, string cacheKey);
    }
}
