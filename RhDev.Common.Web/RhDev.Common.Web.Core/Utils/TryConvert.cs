using System.Data.SqlClient;

namespace RhDev.Common.Web.Core.Utils
{
    public static class TryConvert
    {
        public static string ToString(SqlDataReader obj, string column, string defaultValue = "")
        {
            try
            {
                return Convert.ToString(obj[column]);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static DateTime ToDateTime(SqlDataReader obj, string column, DateTime? defaultValue = null)
        {
            try
            {
                return Convert.ToDateTime(obj[column]);
            }
            catch
            {
                if (defaultValue != null) { return defaultValue.Value; }
                return DateTime.MinValue;
            }
        }

        public static int ToInt32(SqlDataReader obj, string column, int defaultValue = 0)
        {
            try
            {
                return Convert.ToInt32(obj[column]);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static decimal ToDecimal(SqlDataReader obj, string column, decimal defaultValue = 0)
        {
            try
            {
                return Convert.ToDecimal(obj[column]);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
