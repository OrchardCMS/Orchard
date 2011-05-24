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
                (k, currentEntry) => (currentEntry.GetTokens() != null && currentEntry.GetTokens().Any(t => !t.IsCurrent) ? CreateEntry(k, acquire) : currentEntry));

            // Bubble up volatile tokens to parent context
            if (CacheAquireContext.ThreadInstance != null && entry.GetTokens() != null) {
                foreach (var token in entry.GetTokens())
                    CacheAquireContext.ThreadInstance.Monitor(token);
            }

            return entry.Result;
        }


        private static CacheEntry CreateEntry(TKey k, Func<AcquireContext<TKey>, TResult> acquire) {
            var entry = new CacheEntry();
            var context = new AcquireContext<TKey>(k, entry.AddToken);

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
            private IList<IVolatileToken> Tokens { get; set; }

            public void AddToken(IVolatileToken volatileToken) {
                if (Tokens == null) {
                    Tokens = new List<IVolatileToken>();
                }

                Tokens.Add(volatileToken);
            }

            public IEnumerable<IVolatileToken> GetTokens() {
                return Tokens;
            }
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
