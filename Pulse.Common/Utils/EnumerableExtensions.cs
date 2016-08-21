using System;
using System.Collections.Generic;

namespace Pulse.Common.Utils
{
    public static class EnumerableExtensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        public static int IndexOf<T>(this IEnumerable<T> source, T searchItem)
        {
            int index = 0;

            foreach (var item in source)
            {
                if (EqualityComparer<T>.Default.Equals(item, searchItem))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        public static SortedSet<T> ToSortedSet<T>(this IEnumerable<T> source, Comparison<T> compare)
        {
            return new SortedSet<T>(source, new FuncComparer<T>(compare));
        }

        public static T MaxObject<T, U>(this IEnumerable<T> source, Func<T, U> selector)
      where U : IComparable<U>
        {
            if (source == null) throw new ArgumentNullException("source");
            bool first = true;
            T maxObj = default(T);
            U maxKey = default(U);
            foreach (var item in source)
            {
                if (first)
                {
                    maxObj = item;
                    maxKey = selector(maxObj);
                    first = false;
                }
                else
                {
                    U currentKey = selector(item);
                    if (currentKey.CompareTo(maxKey) > 0)
                    {
                        maxKey = currentKey;
                        maxObj = item;
                    }
                }
            }
            if (first) throw new InvalidOperationException("Sequence is empty.");
            return maxObj;
        }

        public static T MinObject<T, U>(this IEnumerable<T> source, Func<T, U> selector)
      where U : IComparable<U>
        {
            if (source == null) throw new ArgumentNullException("source");
            bool first = true;
            T minObj = default(T);
            U minKey = default(U);
            foreach (var item in source)
            {
                if (first)
                {
                    minObj = item;
                    minKey = selector(minObj);
                    first = false;
                }
                else
                {
                    U currentKey = selector(item);
                    if (currentKey.CompareTo(minKey) < 0)
                    {
                        minKey = currentKey;
                        minObj = item;
                    }
                }
            }
            if (first) throw new InvalidOperationException("Sequence is empty.");
            return minObj;
        }
    }

    /*
     * example of usage:
     * SortedSet<string> set = new SortedSet<string>(new FuncComparer<string>((a, b) => a.CompareTo(b)));
     */
    public class FuncComparer<T> : IComparer<T>
    {
        private readonly Comparison<T> _comparison;

        public FuncComparer(Comparison<T> comparison)
        {
            _comparison = comparison;
        }

        public int Compare(T x, T y)
        {
            return _comparison(x, y);
        }
    }

    public class FuncEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _equals;
        private readonly Func<T, int> _getHashCode;

        public FuncEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
        {
            _equals = equals;
            _getHashCode = getHashCode;
        }

        public bool Equals(T x, T y)
        {
            return _equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _getHashCode(obj);
        }
    }
}