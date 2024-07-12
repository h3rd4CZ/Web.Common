using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Utils
{
    public static class PolymorphicBaseConverter
    {
        public static T TryParseValue<T>(object value)
        {
            if (value == default) return default!;

            if (value is JsonElement element)
            {
                Type underlyingType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

                if (element.ValueKind == JsonValueKind.Array)
                {
                    if (!TypeUtils.IsListType<T>()) throw new InvalidOperationException("Element is of array type but T is not of List<> type");
                    var elementType = TypeUtils.GetElementType<T>();

                    var tryParseMethod =
                        typeof(PolymorphicBaseConverter).GetMethod(nameof(TryParseValue))!
                        .MakeGenericMethod(elementType);

                    Type listType = typeof(List<>).MakeGenericType(elementType);
                    object listInstance = Activator.CreateInstance(listType)!;
                    var addMethod = listInstance.GetType().GetMethod("Add")!;

                    foreach (var arrayElement in element.EnumerateArray())
                    {
                        var elementResult = tryParseMethod.Invoke(default, new object[1] { arrayElement });

                        addMethod.Invoke(listInstance, new object[1] { elementResult! });
                    }

                    return (T)listInstance;
                }

                if (underlyingType == typeof(int) && element.TryGetInt32(out int iVal))
                    return (T)Convert.ChangeType(iVal, typeof(T));
                if (underlyingType == typeof(double) && element.TryGetDouble(out double dVal))
                    return (T)Convert.ChangeType(dVal, typeof(T));
                if (underlyingType == typeof(DateTime) && element.TryGetDateTime(out DateTime dtmVal))
                    return (T)Convert.ChangeType(dtmVal, typeof(T));
                if (underlyingType == typeof(decimal) && element.TryGetDecimal(out decimal decVal))
                    return (T)Convert.ChangeType(decVal, typeof(T));
                if (underlyingType == typeof(decimal) && element.TryGetByte(out byte byteVal))
                    return (T)Convert.ChangeType(byteVal, typeof(T));

                if (underlyingType == typeof(bool))
                    return (T)Convert.ChangeType(element.GetBoolean(), typeof(T));
                if (underlyingType == typeof(string))
                    return (T)Convert.ChangeType(element.GetString()!, typeof(T));

                return default!;

            }
            else throw new InvalidOperationException($"Value is not a {typeof(JsonElement).Name}");
        }
    }
}
