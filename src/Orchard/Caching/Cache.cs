using System;
using System.Collections.Generic;
using Orchard.Caching.Providers;

namespace Orchard.Caching {
    public class Cache<TKey, TResult> : ICache<TKey, TResult> {
        private readonly Dictionary<TKey, CacheEntry> _entries;

        public Cache() {
            _entries = new Dictionary<TKey, CacheEntry>();
        }

        public TResult Get(TKey key, Func<AcquireContext<TKey>, TResult> acquire) {
            CacheEntry entry;
            if (!_entries.TryGetValue(key, out entry)) {
                entry = new CacheEntry { VolatileItems = new List<IVolatileSignal>() };

                var context = new AcquireContext<TKey>(key, volatileItem => entry.VolatileItems.Add(volatileItem));
                entry.Result = acquire(context);
                _entries.Add(key, entry);
            }
            return entry.Result;
        }

        private class CacheEntry {
            public TResult Result { get; set; }
            public IList<IVolatileSignal> VolatileItems { get; set; }
        }
    }

}
