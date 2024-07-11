using System;
using System.Globalization;

namespace RhDev.Common.Web.Core.Extensions
{
    public static class DecimalExtensions
    {
        public static string GetCZCurrencyValue(this decimal d)
        {
            return string.Format(new CultureInfo(1029), "{0:c}", d);
        }
    }
}
