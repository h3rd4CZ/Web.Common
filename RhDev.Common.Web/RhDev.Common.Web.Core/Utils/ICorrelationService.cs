using Microsoft.AspNetCore.Authorization;
using RhDev.Common.Web.Core.Composition;

namespace RhDev.Common.Web.Core.Utils
{
    public interface ICorrelationService : IAutoregisteredService
    {
        public string? CurrentCorrelationId { get; }
    }
}
