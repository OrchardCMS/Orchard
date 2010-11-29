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

            // Bubble up volatile tokens to parent context
            if (CacheAquireContext.ThreadInstance != null) {
                foreach (var token in entry.Tokens)
                    CacheAquireContext.ThreadInstance.Monitor(token);
            }

            return entry.Result;
        }


        private static CacheEntry CreateEntry(TKey k, Func<AcquireContext<TKey>, TResult> acquire) {
            var entry = new CacheEntry { Tokens = new List<IVolatileToken>() };
            var context = new AcquireContext<TKey>(k, volatileItem => entry.Tokens.Add(volatileItem));

            IAcquireContext parentContext = null;
            try {
                // Push context
                parentContext = CacheAquireContext.ThreadInstance;
                CacheAquireContext.ThreadInstance = context;

                entry.Result = acquire(context);
            }
            finally {
                // Pop context
                CacheAquireContext.ThreadInstance = parentContext;
            }
            return entry;
        }

        private class CacheEntry {
            public TResult Result { get; set; }
            public IList<IVolatileToken> Tokens { get; set; }
        }
    }

    /// <summary>
    /// Keep track of nested caches contexts on a given thread
    /// </summary>
    internal static class CacheAquireContext {
        [ThreadStatic]
        public static IAcquireContext ThreadInstance;
    }
}
