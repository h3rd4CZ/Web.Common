using System;
using System.Linq;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Utils
{
    public static class ObjectPropertiesBulkAction
    {

        /// <summary>
        /// For each property in object instance of type T set new value.
        /// </summary>
        /// <typeparam name="T">Type of properties</typeparam>
        /// <param name="objectInstance">Instance of object to be edited</param>
        /// <param name="newValueGetter">Function returning new value of type T using property old value</param>
        public static void BulkEditByPropertyType<T>(object objectInstance, Func<T, T> newValueGetter)
        {
            var objectType = objectInstance.GetType();

            // Fix user fields
            foreach (var propertyInfo in objectType.GetProperties().Where(p => p.CanWrite && typeof(T).IsAssignableFrom(p.PropertyType)))
            {
                var property = objectType.GetProperty(propertyInfo.Name);
                var oldValue = (T)property.GetValue(objectInstance);
                property.SetValue(objectInstance, newValueGetter(oldValue));
            }
        }

        /// <summary>
        /// For each property in object instance of type T set new value.
        /// </summary>
        /// <typeparam name="T">Type of properties</typeparam>
        /// <param name="objectInstance">Instance of object to be edited</param>
        /// <param name="newValueGetter">Function returning new value of type T using property old value</param>
        public static async Task BulkEditByPropertyTypeAsync<T>(object objectInstance, Func<T, Task<T>> newValueGetter)
        {
            var objectType = objectInstance.GetType();

            // Fix user fields
            foreach (var propertyInfo in objectType.GetProperties().Where(p => p.CanWrite && typeof(T).IsAssignableFrom(p.PropertyType)))
            {
                var property = objectType.GetProperty(propertyInfo.Name);
                var oldValue = (T)property.GetValue(objectInstance);
                property.SetValue(objectInstance, await newValueGetter(oldValue));
            }
        }
    }
}
