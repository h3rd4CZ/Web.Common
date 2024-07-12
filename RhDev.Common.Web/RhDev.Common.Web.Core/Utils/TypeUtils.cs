using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Utils
{
    public static class TypeUtils
    {
        public static bool IsListType<T>()
        {
            var type = typeof(T);

            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsCollectionType<T>()
        {
            Type type = typeof(T);

            if (type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                return true;
            }

            if (type.IsArray)
            {
                return true;
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return true;
            }

            return false;
        }

        public static Type GetElementType<T>()
        {
            Type type = typeof(T);

            if (type.IsArray)
            {
                return type.GetElementType();
            }

            Type enumerableInterface = type.GetInterfaces().FirstOrDefault(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            if (enumerableInterface != null)
            {
                return enumerableInterface.GetGenericArguments().FirstOrDefault();
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return typeof(object);
            }

            throw new InvalidOperationException("Type is not a collection type.");
        }
    }
}
