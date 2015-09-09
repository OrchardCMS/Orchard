using System;

namespace Orchard.Caching {
    public interface ICacheManager {
        TResult Get<TKey, TResult>(TKey key, Func<AcquireContext<TKey>, TResult> acquire);
        ICache<TKey, TResult> GetCache<TKey, TResult>();
    }

    public static class CacheManagerExtensions {
        public static TResult Get<TKey, TResult>(this ICacheManager cacheManager, TKey key, bool lazy, Func<AcquireContext<TKey>, TResult> acquire) {
            // Wrap the call in a Lazy initializer to prevent multiple processes from
            // executing the same lambda in parallel.
            if (lazy) {
                return cacheManager.Get<TKey, Lazy<TResult>>(key, k => new Lazy<TResult>(() => acquire(k))).Value;
            }
            else {
                return cacheManager.Get(key, acquire);
            }
        }
    }
}
