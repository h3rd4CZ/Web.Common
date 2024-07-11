using Microsoft.Extensions.Primitives;

namespace RhDev.Common.Web.Core.Utils
{
    public interface ISqlServerWatcher : IDisposable
    {
        IChangeToken Watch();
    }
}
