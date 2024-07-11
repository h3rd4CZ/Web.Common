using System.Collections.Generic;
using System.Linq;

namespace RhDev.Common.Web.Core.Extensions
{
    public static class ICollectionExtensions
    {
        public static bool UnorderedEqual<T>(this ICollection<T> a, ICollection<T> b)
        {
            if (Equals(null, a) && Equals(null, b)) return true;

            if (!(!Equals(null, a) && !Equals(null, b))) return false;

            if (a.Count != b.Count)
            {
                return false;
            }
            // 2
            // Initialize new Dictionary of the type
            Dictionary<T, int> d = new Dictionary<T, int>();
            // 3
            // Add each key's frequency from collection A to the Dictionary
            foreach (T item in a)
            {
                int c;
                if (d.TryGetValue(item, out c))
                {
                    d[item] = c + 1;
                }
                else
                {
                    d.Add(item, 1);
                }
            }
            // 4
            // Add each key's frequency from collection B to the Dictionary
            // Return early if we detect a mismatch
            foreach (T item in b)
            {
                int c;
                if (d.TryGetValue(item, out c))
                {
                    if (c == 0)
                    {
                        return false;
                    }
                    else
                    {
                        d[item] = c - 1;
                    }
                }
                else
                {
                    // Not in dictionary
                    return false;
                }
            }
            // 5
            // Verify that all frequencies are zero
            return d.Values.All(v => v == 0);
            // 6
            // We know the collections are equal
        }
    }
}
