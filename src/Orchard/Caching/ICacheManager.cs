using System;
using System.Collections.Concurrent;

namespace Orchard.Caching {
    public interface ICacheManager {
        TResult Get<TKey, TResult>(TKey key, Func<AcquireContext<TKey>, TResult> acquire);
        ICache<TKey, TResult> GetCache<TKey, TResult>();
    }

    public static class CacheManagerExtensions {

        private static readonly ConcurrentDictionary<object, object> _locks = new ConcurrentDictionary<object, object>();

        public static TResult Get<TKey, TResult>(this ICacheManager cacheManager, TKey key, bool preventConcurrentCalls, Func<AcquireContext<TKey>, TResult> acquire) {
            if (preventConcurrentCalls) {
                var lockKey = _locks.GetOrAdd(key, _ => new object());
                lock (lockKey) {
                    return cacheManager.Get(key, acquire);
                }
            }
            else {
                return cacheManager.Get(key, acquire);
            }
        }
    }
}
