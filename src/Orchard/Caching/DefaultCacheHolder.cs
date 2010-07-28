using System;
using System.Collections.Concurrent;

namespace Orchard.Caching {
    public class DefaultCacheHolder : ICacheHolder {
        private readonly ConcurrentDictionary<CacheKey, object> _caches = new ConcurrentDictionary<CacheKey, object>();

        class CacheKey : Tuple<Type, Type, Type> {
            public CacheKey(Type component, Type key, Type result)
                : base(component, key, result) {
            }
        }

        public ICache<TKey, TResult> GetCache<TKey, TResult>(Type component) {
            var cacheKey = new CacheKey(component, typeof(TKey), typeof(TResult));
            var result = _caches.GetOrAdd(cacheKey, k => new Cache<TKey, TResult>());
            return (Cache<TKey, TResult>)result;
        }
    }
}