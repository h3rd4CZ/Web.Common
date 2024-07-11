using RhDev.Common.Web.Core.Composition;

namespace RhDev.Common.Web.Core
{
    public interface ICentralClockProvider : IAutoregisteredService
    {
        CentralClock UtcNow();
        CentralClock Now();
        long UtcUnixSecondsNow();
    }
}
