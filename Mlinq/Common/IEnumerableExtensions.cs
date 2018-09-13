using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mlinq.Common
{
    [DebuggerStepThrough]
    internal static class IEnumerableExtensions
    {
        public static string Uniquify(this IEnumerable<string> inputStrings, string targetString)
        {
            var uniqueString = targetString;
            var i = 0;

            while (inputStrings.Any(n => string.Equals(n, uniqueString, StringComparison.Ordinal)))
            {
                uniqueString = targetString + ++i;
            }

            return uniqueString;
        }

        public static void Each<T>(this IEnumerable<T> ts, Action<T, int> action)
        {
            var i = 0;
            foreach (var t in ts)
            {
                action(t, i++);
            }
        }

        public static void Each<T>(this IEnumerable<T> ts, Action<T> action)
        {
            foreach (var t in ts)
            {
                action(t);
            }
        }

        public static void Each<T, S>(this IEnumerable<T> ts, Func<T, S> action)
        {
            foreach (var t in ts)
            {
                action(t);
            }
        }

        public static string Join<T>(this IEnumerable<T> ts, Func<T, string> selector = null, string separator = ", ")
        {
            selector = selector ?? (t => t.ToString());

            return string.Join(separator, ts.Where(t => !ReferenceEquals(t, null)).Select(selector));
        }

        public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource value)
        {
            yield return value;

            foreach (var element in source)
            {
                yield return element;
            }
        }

        public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> source, TSource value)
        {
            foreach (var element in source)
            {
                yield return element;
            }

            yield return value;
        }
    }
}
