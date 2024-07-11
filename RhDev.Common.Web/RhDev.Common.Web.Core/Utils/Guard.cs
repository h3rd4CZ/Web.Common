using System.Runtime.CompilerServices;

namespace RhDev.Common.Web.Core.Utils
{
    public static class Guard
    {
        public static void NotDefault<T>(T parameter, string parameterName, string message = default)
        {
            if (Equals(default(T), parameter))
                throw new InvalidOperationException($"Parameter : {parameterName} has default value : {(string.IsNullOrWhiteSpace(message) ? string.Empty : message)}");
        }

        public static void NotDefault<T>(T parameter, [CallerArgumentExpression("parameter")] string argumentName = default)
        {
            if (Equals(default(T), parameter))
                throw new InvalidOperationException($"Parameter : {argumentName} has default value");
        }

        public static void NotNull(object parameter, string parameterName, string message = default)
        {
            if (Equals(null, parameter)) throw new ArgumentNullException(parameterName, message ?? string.Empty);
        }

        public static void NotNull(object parameter, [CallerArgumentExpression("parameter")] string argumentName = default)
        {
            if (Equals(null, parameter)) throw new ArgumentNullException(argumentName);
        }

        public static void CollectionNotNullAndNotEmpty<T>(ICollection<T> collection, string argumentName, string message = default)
        {
            NotNull(collection, argumentName, message);

            if (collection.Count == 0) throw new ArgumentNullException($"Collection {argumentName} is empty", message ?? string.Empty);
        }

        public static void CollectionMustBeNullAndEmpty<T>(ICollection<T> collection, string argumentName, string message = default)
        {
            if (Equals(null, collection) || collection.Count == 0) return;

            throw new ArgumentNullException($"Collection {argumentName} is not empty", message ?? string.Empty);
        }

        public static void CollectionHasExactlyNumberElements<T>(ICollection<T> collection, int numElements, string argumentName, string message = default)
        {
            CollectionNotNullAndNotEmpty(collection, argumentName, message);

            if (collection.Count != numElements) throw new ArgumentNullException($"Collection {argumentName} has not exactly {numElements} elements", message ?? string.Empty);
        }

        public static void CollectionHasOneElement<T>(ICollection<T> collection, string argumentName, string message = default)
        {
            CollectionNotNullAndNotEmpty(collection, argumentName, message);

            if (collection.Count != 1) throw new ArgumentNullException($"Collection {argumentName} does not have one element", message ?? string.Empty);
        }

        public static void StringNotNullOrEmpty(string s, string parameterName, string message = default)
        {
            if (string.IsNullOrEmpty(s)) throw new ArgumentNullException(parameterName, message ?? string.Empty);
        }

        public static void StringNotNullOrWhiteSpace(string s, string parameterName, string message = default)
        {
            if (string.IsNullOrWhiteSpace(s)) throw new ArgumentNullException(parameterName, message ?? string.Empty);
        }

        public static void StringNotNullOrWhiteSpace(string s, [CallerArgumentExpression("s")] string argumentName = default)
        {
            if (string.IsNullOrWhiteSpace(s)) throw new ArgumentNullException(argumentName);
        }

        public static void NumberInRange<T>(T number, T min, T max, string argumentName) where T : IComparable<T>
        {

            if (number.CompareTo(min) < 0 || number.CompareTo(max) > 0)
                throw new ArgumentOutOfRangeException(argumentName);
        }

        public static void NumberMin<T>(T number, T min, [CallerArgumentExpression("number")] string argumentName = default) where T : IComparable<T>
        {
            if (number.CompareTo(min) < 0) throw new ArgumentOutOfRangeException(argumentName);
        }

        public static void NumberAtLeast<T>(T number, T least, [CallerArgumentExpression("number")] string argumentName = default) where T : IComparable<T>
        {
            if (number.CompareTo(least) < 0) throw new ArgumentOutOfRangeException(argumentName);
        }

        public static void NumberMax<T>(T number, T max, string argumentName) where T : IComparable<T>
        {
            if (number.CompareTo(max) > 0) throw new ArgumentOutOfRangeException(argumentName);
        }

        public static void IsEnumEqualTo<T>(T obj, T obj2) where T : Enum
        {
            if (!Equals(obj, obj2)) throw new ArgumentOutOfRangeException($"Enum {typeof(T)} (has value {Enum.GetName(typeof(T), obj)}) and does not equal to {Enum.GetName(typeof(T), obj2)}");
        }

        public static void IsEqual<T>(T obj, T obj2, string parameterName, string message = default) where T : struct
        {
            if (!Equals(obj, obj2)) throw new ArgumentOutOfRangeException(parameterName, message ?? string.Empty);
        }

        public static void IsTrue(bool parameter, string parameterName, string message = default)
        {
            if (!Equals(parameter, true)) throw new ArgumentOutOfRangeException(parameterName, message ?? string.Empty);
        }
    }

}
