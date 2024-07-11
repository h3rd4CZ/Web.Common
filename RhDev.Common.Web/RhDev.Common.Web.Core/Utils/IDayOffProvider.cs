using RhDev.Common.Web.Core;
using RhDev.Common.Web.Core.Composition;

namespace RhDev.Common.Web.Core.Utils
{
    public interface IDayOffProvider : IAutoregisteredService
    {
        Task<int> GetDayOffsCountFromDateRangeAsync(CentralClock from, CentralClock to, IList<DayOfWeek> exceptList = null);
        Task<bool> IsDayOffAsync(DateTime now, bool includeWeekend = false);
        Task<IList<DateTime>> GetAllDayOffsAsync();
    }
}
