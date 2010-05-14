using System;
using System.Collections.Generic;

namespace Orchard.Caching {
    public class Cache<TKey, TResult> : ICache<TKey, TResult> {
        private readonly Dictionary<TKey, CacheEntry> _entries;

        public Cache() {
            _entries = new Dictionary<TKey, CacheEntry>();
        }

        #region Implementation of ICache<TKey,TResult>

        public TResult Get(TKey key, Func<AcquireContext, TResult> acquire) {
            CacheEntry entry;
            if (!_entries.TryGetValue(key, out entry)) {
                AcquireContext context = new AcquireContext();
                entry = new CacheEntry {Result = acquire(context)};
                _entries.Add(key, entry);
            }
            return entry.Result;
        }

        #endregion

        public class CacheEntry {
            public TResult Result { get; set; }
        }
    }

}
