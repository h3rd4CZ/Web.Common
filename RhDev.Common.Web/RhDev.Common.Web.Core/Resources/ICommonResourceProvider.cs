using RhDev.Common.Web.Core.Composition;
using System.Globalization;

namespace RhDev.Common.Web.Core.Resources
{
    public interface ICommonResourceProvider : IAutoregisteredService
    {
        string GetString<TResource>(string key, CultureInfo ci = default) where TResource : class;
    }
}
