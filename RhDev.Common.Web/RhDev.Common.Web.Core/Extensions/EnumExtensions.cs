using RhDev.Common.Web.Core.Extensions;
using System.ComponentModel;

namespace RhDev.Common.Web.Core.Extensions
{
    public static class EnumExtensions
    {
        public static bool HasFlag(this Enum variable, Enum value)
        {
            if (variable == null)
                return false;

            if (value == null)
                throw new ArgumentNullException("value");

            // Not as good as the .NET 4 version of this function, but should be good enough
            if (!Enum.IsDefined(variable.GetType(), value))
            {
                throw new ArgumentException(string.Format(
                    "Enumeration type mismatch.  The flag is of type '{0}', was expecting '{1}'.",
                    value.GetType(), variable.GetType()));
            }

            ulong num = Convert.ToUInt64(value);
            return (Convert.ToUInt64(variable) & num) == num;

        }

        public static IEnumerable<Enum> GetFlags(this Enum e)
        {
            return Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag);
        }

        public static T GetEnumType<T>(this string str) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            var enumType = typeof(T);
            foreach (var enumVal in Enum.GetValues(enumType))
            {
                var memInfo = enumType.GetMember(enumVal.ToString());
                var attr = (DescriptionAttribute)memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
                if (attr != null && attr.Description.Equals(str, StringComparison.OrdinalIgnoreCase))
                    return (T)enumVal;
            }

            return default;
        }

        public static string GetEnumDescription(this Enum enumObject)
        {
            var type = enumObject.GetType();
            var memInfo = type.GetMember(enumObject.ToString());

            if (memInfo.Length <= 0)
            {
                return enumObject.ToString();
            }

            var attributes = (DescriptionAttribute[])memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                attributes = attributes.OfType<DescriptionAttribute>().Where(a => !a.GetType().IsSubclassOf(typeof(DescriptionAttribute))).ToArray();
            }
            return attributes.Length > 0
                ? attributes[0].Description
                : enumObject.ToString();
        }
    }
}
