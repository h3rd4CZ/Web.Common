using System;

namespace RhDev.Common.Web.Core
{
    [Serializable]
    public class CentralClock
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
        public DateTimeKind Kind { get; set; }

        public static CentralClock FillFromDateTime(DateTime dtm)
        {
            var cc = new CentralClock();
            cc.FillFrom(dtm);

            return cc;
        }

        public static CentralClock FillFromDateTimeOffset(DateTimeOffset dtm)
             => FillFromDateTime(dtm.Date);

        public void FillFrom(DateTime dtm)
        {
            Year = dtm.Year;
            Month = dtm.Month;
            Day = dtm.Day;
            Hour = dtm.Hour;
            Minute = dtm.Minute;
            Second = dtm.Second;
            Kind = dtm.Kind;
        }

        public static bool operator >(CentralClock left, CentralClock right) => left.ExportDateTime > right.ExportDateTime;
        public static bool operator <(CentralClock left, CentralClock right) => left.ExportDateTime < right.ExportDateTime;
        public static bool operator <=(CentralClock left, CentralClock right) => left.ExportDateTime <= right.ExportDateTime;
        public static bool operator >=(CentralClock left, CentralClock right) => left.ExportDateTime >= right.ExportDateTime;

        public DateTimeOffset ExportDateTimeOffset => new DateTimeOffset(ExportDateTime);
        public DateTime ExportDateTime => new DateTime(Year, Month, Day, Hour, Minute, Second, Kind);
        public TimeOnly ExportTime => new TimeOnly(Hour, Minute, Second);
        public DateOnly ExportToday => new DateOnly(Year, Month, Day);
        public DateTime ExportTodayDateTime => new DateTime(Year, Month, Day, 0, 0, 0, Kind);
        public DateTimeOffset ExportTodayStartDateTime => new DateTimeOffset(Year, Month, Day, 0, 0, 0, 0, TimeSpan.Zero);
        public DateTimeOffset ExportTodayEndDateTime => new DateTimeOffset(Year, Month, Day, 23, 59, 59, 999, TimeSpan.Zero);
        public DateTimeOffset ExportTodayDateTimeOffset => new DateTimeOffset(ExportTodayDateTime);

        public bool IsWeekend
            => ExportDateTime.DayOfWeek == DayOfWeek.Saturday || ExportDateTime.DayOfWeek == DayOfWeek.Sunday;

        public bool IsInTimeRange(TimeOnly from, TimeOnly to)
        {
            var time = ExportTime;

            return time >= from && time <= to;
        }

        public override string ToString()
        {
            var dtm = ExportDateTime;

            return dtm.ToString();
        }

        public string ToString(IFormatProvider provider)
        {
            var dtm = ExportDateTime;

            return dtm.ToString(provider);
        }

        public string ToLongTimeString()
        {
            var dtm = ExportDateTime;

            return dtm.ToLongTimeString();
        }

        public string ToLongDateString()
        {
            var dtm = ExportDateTime;

            return dtm.ToLongDateString();
        }
    }
}
