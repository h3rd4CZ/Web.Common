using System;
using System.Collections.Generic;
using System.Text;

namespace RhDev.Common.Workflow.Extensions
{
    public static class DateTimeExtension
    {
        public static DateTime AddBusinessDays(this DateTime date, int days)
        {
            int direction = days < 0 ? -1 : 1;
            DateTime newDate = date;
            while (days != 0)
            {
                newDate = newDate.AddDays(direction);
                if (newDate.DayOfWeek != DayOfWeek.Saturday &&
                    newDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    days -= direction;
                }
            }
            return newDate;

        }

        public static DateTime ChangeTime(this DateTime dateTime, int hours, int minutes, int seconds, int milliseconds)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                hours,
                minutes,
                seconds,
                milliseconds,
                dateTime.Kind);
        }
    }
}
