using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Caching {
    public class Cache<TKey, TResult> : ICache<TKey, TResult> {
        private readonly Dictionary<TKey, CacheEntry> _entries;

        public Cache() {
            _entries = new Dictionary<TKey, CacheEntry>();
        }

        public TResult Get(TKey key, Func<AcquireContext<TKey>, TResult> acquire) {
            CacheEntry entry;
            if (!_entries.TryGetValue(key, out entry) || entry.Tokens.Any(t => !t.IsCurrent)) {
                entry = new CacheEntry { Tokens = new List<IVolatileToken>() };

                var context = new AcquireContext<TKey>(key, volatileItem => entry.Tokens.Add(volatileItem));
                entry.Result = acquire(context);
                _entries[key] = entry;
            }
            return entry.Result;
        }

        private class CacheEntry {
            public TResult Result { get; set; }
            public IList<IVolatileToken> Tokens { get; set; }
        }
    }

}
