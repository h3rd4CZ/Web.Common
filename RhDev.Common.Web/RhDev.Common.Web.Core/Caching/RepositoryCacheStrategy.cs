using System;

namespace RhDev.Common.Web.Core.Caching
{
    [Flags]
    public enum RepositoryCacheStrategy
    {
        ReadOnly = 1,
        InvalidateAffectedOnWrite = 1 << 1,
        InvalidateAllOnWrite = 1 << 2,
        AlwaysBypass = 1 << 3,
        WriteSensitive = InvalidateAffectedOnWrite | InvalidateAllOnWrite
    }
}
