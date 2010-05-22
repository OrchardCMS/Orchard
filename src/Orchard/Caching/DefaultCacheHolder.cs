using System;
using System.Collections.Generic;
using Castle.Core;

namespace Orchard.Caching {
    public class DefaultCacheHolder : ICacheHolder {
        private readonly IDictionary<CacheKey, object> _caches = new Dictionary<CacheKey, object>();

        class CacheKey : Pair<Type, Pair<Type, Type>> {
            public CacheKey(Type component, Type key, Type result)
                : base(component, new Pair<Type, Type>(key, result)) {
            }
        }

        public ICache<TKey, TResult> GetCache<TKey, TResult>(Type component) {
            var cacheKey = new CacheKey(component, typeof(TKey), typeof(TResult));
            lock (_caches) {
                object value;
                if (!_caches.TryGetValue(cacheKey, out value)) {
                    value = new Cache<TKey, TResult>();
                    _caches[cacheKey] = value;
                }
                return (ICache<TKey, TResult>)value;
            }
        }
    }
}