using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Caching {
    public class Cache<TKey, TResult> : ICache<TKey, TResult> {
        private readonly IAcquireContextContext _acquireContextContext;
        private readonly ConcurrentDictionary<TKey, CacheEntry> _entries;

        public Cache(IAcquireContextContext acquireContextContext) {
            _acquireContextContext = acquireContextContext;
            _entries = new ConcurrentDictionary<TKey, CacheEntry>();
        }

        public TResult Get(TKey key, Func<AcquireContext<TKey>, TResult> acquire) {
            var entry = _entries.AddOrUpdate(key,
                // "Add" lambda
                k => CreateEntry(k, acquire),
                // "Update" lamdba
                (k, currentEntry) => (currentEntry.GetTokens() != null && currentEntry.GetTokens().Any(t => !t.IsCurrent) ? CreateEntry(k, acquire) : currentEntry));

            // Bubble up volatile tokens to parent context
            if (_acquireContextContext.Instance != null && entry.GetTokens() != null) {
                foreach (var token in entry.GetTokens())
                    _acquireContextContext.Instance.Monitor(token);
            }

            return entry.Result;
        }


        private CacheEntry CreateEntry(TKey k, Func<AcquireContext<TKey>, TResult> acquire) {
            var entry = new CacheEntry();
            var context = new AcquireContext<TKey>(k, entry.AddToken);

            IAcquireContext parentContext = null;
            try {
                // Push context
                parentContext = _acquireContextContext.Instance;
                _acquireContextContext.Instance = context;

                entry.Result = acquire(context);
            }
            finally {
                // Pop context
                _acquireContextContext.Instance = parentContext;
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
}
