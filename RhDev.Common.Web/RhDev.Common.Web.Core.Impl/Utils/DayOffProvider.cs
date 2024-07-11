using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Stores.Utils;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Web.Core.Impl.Utils
{
    public class DayOffProvider : IDayOffProvider
    {
        private readonly IDataStoreAcessRepositoryFactory dataStoreAcessRepositoryFactory;

        public DayOffProvider(IDataStoreAcessRepositoryFactory dataStoreAcessRepositoryFactory)
        {
            this.dataStoreAcessRepositoryFactory = dataStoreAcessRepositoryFactory;
        }

        public async Task<int> GetDayOffsCountFromDateRangeAsync(CentralClock from, CentralClock to, IList<DayOfWeek> exceptList = null)
        {
            var count = 0;

            var fromDate = from.ExportDateTime;
            var toDate = to.ExportDateTime;

            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                if (!Equals(null, exceptList) && exceptList.Contains(date.DayOfWeek))
                    continue;

                if (await IsDayOffAsync(date))
                    count++;
            }

            return count;
        }

        public async Task<bool> IsDayOffAsync(DateTime now, bool includeWeekend = false)
        {
            var date = now;

            if (includeWeekend && date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) return true;

            var repo
                = dataStoreAcessRepositoryFactory.GetDomainQueryableStoreRepository<IDayOffDataStore>(Core.Caching.RepositoryCacheStrategy.ReadOnly);

            var allDayOffs = await repo.ReadAllAsync();

            var datePublicHolidays = allDayOffs.Where(x => x.Day.Day == date.Day && x.Day.Month == date.Month).ToList();

            if (datePublicHolidays.Count == 0) return false;

            foreach (var datePublicHoliday in datePublicHolidays)
            {
                if (datePublicHoliday.Repeat)
                    return true;

                if (datePublicHoliday.Day.Year == date.Year)
                    return true;
            }

            return false;
        }

        public async Task<IList<DateTime>> GetAllDayOffsAsync()
        {
            var repo
                = dataStoreAcessRepositoryFactory.GetDomainQueryableStoreRepository<IDayOffDataStore>(Core.Caching.RepositoryCacheStrategy.ReadOnly);

            var allDayOffs = new List<DateTime>();

            var allDayOffsData = await repo.ReadAllAsync();

            allDayOffs.AddRange(allDayOffsData.Where(d => !d.Repeat).Select(d => d.Day));

            var currYear = DateTime.Now.Year;

            foreach (var repeatDay in allDayOffsData.Where(d => d.Repeat))
            {
                allDayOffs
                    .AddRange(Enumerable.Range(0, 10)
                    .Select(r => new DateTime(currYear + r, repeatDay.Day.Month, repeatDay.Day.Day)));
            }

            return allDayOffs;
        }
    }
}
