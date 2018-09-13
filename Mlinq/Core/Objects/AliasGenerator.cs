using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace Mlinq.Core.Objects
{
    internal sealed class AliasGenerator
    {
        private const int MaxPrefixCount = 500;

        private const int CacheSize = 250;

        private static readonly string[] _counterNames = new string[CacheSize];

        private static Dictionary<string, string[]> _prefixCounter;

        private int _counter;
        private readonly string _prefix;
        private readonly string[] _cache;

        internal AliasGenerator(string prefix)
            : this(prefix, CacheSize)
        {
        }

        internal AliasGenerator(string prefix, int cacheSize)
        {
            _prefix = prefix ?? String.Empty;

            if (0 < cacheSize)
            {
                string[] cache = null;
                Dictionary<string, string[]> updatedCache;
                Dictionary<string, string[]> prefixCounter;
                while ((null == (prefixCounter = _prefixCounter)) || !prefixCounter.TryGetValue(prefix, out _cache))
                {
                    if (null == cache)
                    {
                        cache = new string[cacheSize];
                    }

                    var capacity = 1 + ((null != prefixCounter) ? prefixCounter.Count : 0);
                    updatedCache = new Dictionary<string, string[]>(capacity, StringComparer.InvariantCultureIgnoreCase);
                    if ((null != prefixCounter) && (capacity < MaxPrefixCount))
                    {
                        foreach (var entry in prefixCounter)
                        {
                            updatedCache.Add(entry.Key, entry.Value);
                        }
                    }
                    updatedCache.Add(prefix, cache);
                    Interlocked.CompareExchange(ref _prefixCounter, updatedCache, prefixCounter);
                }
            }
        }

        internal string Next()
        {
            _counter = Math.Max(unchecked(1 + _counter), 0);
            return GetName(_counter);
        }

        internal string GetName(int index)
        {
            string name;
            if ((null == _cache) || unchecked((uint)_cache.Length <= (uint)index))
            {
                name = String.Concat(_prefix, index.ToString(CultureInfo.InvariantCulture));
            }
            else if (null == (name = _cache[index]))
            {
                if (unchecked((uint)_counterNames.Length <= (uint)index))
                {
                    name = index.ToString(CultureInfo.InvariantCulture);
                }
                else if (null == (name = _counterNames[index]))
                {
                    _counterNames[index] = name = index.ToString(CultureInfo.InvariantCulture);
                }
                _cache[index] = name = String.Concat(_prefix, name);
            }
            return name;
        }
    }
}
