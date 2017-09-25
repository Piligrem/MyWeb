using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace InSearch
{
    public static class CollectionExtensions
    {
        //
        public static void AddRange<T>(this ICollection<T> initial, IEnumerable<T> other)
        {
            if (other == null)
                return;

            var list = initial as List<T>;

            if (list != null)
            {
                list.AddRange(other);
                return;
            }

            other.Each(x => initial.Add(x));
        }

        public static bool IsNullOrEmpty<T>(this ICollection<T> source)
        {
            return (source == null || source.Count == 0);
        }

        public static bool EqualsAll<T>(this IList<T> a, IList<T> b)
        {
            if (a == null || b == null)
                return (a == null && b == null);

            if (a.Count != b.Count)
                return false;

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;

            for (int i = 0; i < a.Count; i++)
            {
                if (!comparer.Equals(a[i], b[i]))
                    return false;
            }

            return true;
        }

        public static T GetNext<T>(this IList<T> collection, T value, bool round)
        {
            int nextIndex = collection.IndexOf(value) + 1;
            if (round && nextIndex >= collection.Count)
                nextIndex = 0;

            if (nextIndex < collection.Count)
            {
                return collection[nextIndex];
            }
            else
            {
                return value; //Or throw an exception
            }
        }
    }
}
