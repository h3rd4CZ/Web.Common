using System;

namespace RhDev.Common.Web.Core.Extensions
{
    public static class DateTimeExtensions
    {
        private const string DATE_TIME_STRING_FORMAT = "d.M.yyyy (HH:mm)";
        private const string DATE_STRING_FORMAT = "d.M.yyyy";

        public static DateTimeOffset? ToLocalDateTimeOffset(this DateTime? dateTime, bool endDay = false)
        {
            if (dateTime is null) return default;

            return dateTime.Value.ToLocalDateTimeOffset(endDay);
        }

        public static DateTimeOffset ToLocalDateTimeOffset(this DateTime dateTime, bool endDay = false)
        {
            var zoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);

            var dtm = dateTime;

            return new DateTimeOffset(new DateTime(dtm.Year, dtm.Month, dtm.Day, endDay ? 23 : dtm.Hour, endDay ? 59 : dtm.Minute, endDay ? 59 : dtm.Second), zoneOffset);
        }

        public static DateTimeOffset TransformLocalDateTimeOffset(this DateTimeOffset dateTimeOffset)
        {
            var zoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);

            return new DateTimeOffset(new DateTime(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, dateTimeOffset.Hour, dateTimeOffset.Minute, dateTimeOffset.Second), zoneOffset);
        }

        public static DateTimeOffset ToEndOfDayOffset(this DateTimeOffset dateTimeOffset)
        {
            return new DateTimeOffset(
                new DateTime(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, 23, 59, 59), dateTimeOffset.Offset);
        }

        public static DateTimeOffset ToStartOfDayOffset(this DateTimeOffset dateTimeOffset)
        {
            return new DateTimeOffset(
                new DateTime(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, 0, 0, 0), dateTimeOffset.Offset);
        }

        public static string ToDateString(this DateTime? dateTime)
        {
            if (dateTime.HasValue)
                return dateTime.Value.ToString(DATE_STRING_FORMAT);

            return string.Empty;
        }

        public static string ToDateTimeString(this DateTime? dateTime)
        {
            if (dateTime.HasValue)
                return dateTime.Value.ToString(DATE_TIME_STRING_FORMAT);

            return string.Empty;
        }

        public static string ToDateString(this DateTime dateTime)
        {
            return dateTime.ToString(DATE_STRING_FORMAT);
        }

        public static DateOnly GetDateOnly(this DateTime dateTime)
        {
            return new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
        }

        public static DateTime GetDateTime(this DateOnly date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
        }

        public static DateTime GetDateTime(this DateOnly date, TimeOnly time)
        {
            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
        }

        public static TimeOnly GetTimeOnly(this DateTime dateTime)
        {
            var timeSpan = dateTime.TimeOfDay;

            return new TimeOnly(timeSpan.Ticks);
        }

        public static string ToDateTimeString(this DateTime dateTime)
        {
            return dateTime.ToString(DATE_TIME_STRING_FORMAT);
        }

        public static DateTime TrimToDayOnly(this DateTime dtm)
        {
            return new DateTime(dtm.Year, dtm.Month, dtm.Day, dtm.Hour, 0, 0);
        }
    }
}
