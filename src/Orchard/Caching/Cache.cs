using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Caching {
    public class Cache<TKey, TResult> : ICache<TKey, TResult> {
        private readonly ConcurrentDictionary<TKey, CacheEntry> _entries;

        public Cache() {
            _entries = new ConcurrentDictionary<TKey, CacheEntry>();
        }

        public TResult Get(TKey key, Func<AcquireContext<TKey>, TResult> acquire) {
            var entry = _entries.AddOrUpdate(key,
                // "Add" lambda
                k => CreateEntry(k, acquire),
                // "Update" lamdba
                (k, currentEntry) => (currentEntry.Tokens.All(t => t.IsCurrent) ? currentEntry : CreateEntry(k, acquire)));
            return entry.Result;
        }

        private static CacheEntry CreateEntry(TKey k, Func<AcquireContext<TKey>, TResult> acquire) {
            var entry = new CacheEntry { Tokens = new List<IVolatileToken>() };
            var context = new AcquireContext<TKey>(k, volatileItem => entry.Tokens.Add(volatileItem));
            entry.Result = acquire(context);
            return entry;
        }

        private class CacheEntry {
            public TResult Result { get; set; }
            public IList<IVolatileToken> Tokens { get; set; }
        }
    }
}
