using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Utils
{
    public static class ObjectAccessor
    {
        public static bool IsNestedObjectAccessor(string property) => property is not null && property.Contains(".");

        public static (object? propertyValue, Type propertyType, PropertyInfo pi) GetPublicPropertyValue(object src, string propName)
        {
            if (src is null) return (default, default!, default!);

            Guard.StringNotNullOrWhiteSpace(propName);

            if (propName.Contains("."))
            {
                var temp = propName.Split(new char[] { '.' }, 2);

                return GetPublicPropertyValue(GetPublicPropertyValue(src, temp[0]).propertyValue, temp[1]);
            }
            else
            {
                var pi = src.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public);

                Guard.NotNull(pi, nameof(pi), $"Property was not found : {propName}");

                return (pi.GetValue(src, null)!, pi.PropertyType, pi);
            }
        }
    }
}
