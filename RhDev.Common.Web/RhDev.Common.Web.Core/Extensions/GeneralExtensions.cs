using System.ComponentModel;
using System.Linq;

namespace RhDev.Common.Web.Core.Extensions
{
    public static class GeneralExtensions
    {
        public static string GetDescription(this object obj)
        {
            var type = obj.GetType();
            var memInfo = type.GetMember(obj.ToString());

            if (memInfo.Length <= 0)
            {
                return string.Empty;
            }

            var attributes = (DescriptionAttribute[])memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                attributes = attributes.OfType<DescriptionAttribute>().Where(a => !a.GetType().IsSubclassOf(typeof(DescriptionAttribute))).ToArray();
            }
            return attributes.Length > 0
                ? attributes[0].Description
                : string.Empty;
        }
    }
}
