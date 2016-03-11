using System;
using System.Collections.Concurrent;

namespace Orchard.Caching {
    /// <summary>
    /// Provides the default implementation for a cache holder.
    /// The cache holder is responsible for actually storing the references to cached entities.
    /// </summary>
    public class DefaultCacheHolder : ICacheHolder {
        private readonly ICacheContextAccessor _cacheContextAccessor;
        private readonly ConcurrentDictionary<CacheKey, object> _caches = new ConcurrentDictionary<CacheKey, object>();

        public DefaultCacheHolder(ICacheContextAccessor cacheContextAccessor) {
            _cacheContextAccessor = cacheContextAccessor;
        }

        class CacheKey : Tuple<Type, Type, Type> {
            public CacheKey(Type component, Type key, Type result)
                : base(component, key, result) {
            }
        }

        /// <summary>
        /// Gets a Cache entry from the cache. If none is found, an empty one is created and returned.
        /// </summary>
        /// <typeparam name="TKey">The type of the key within the component.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="component">The component context.</param>
        /// <returns>An entry from the cache, or a new, empty one, if none is found.</returns>
        public ICache<TKey, TResult> GetCache<TKey, TResult>(Type component) {
            var cacheKey = new CacheKey(component, typeof(TKey), typeof(TResult));
            var result = _caches.GetOrAdd(cacheKey, k => new Cache<TKey, TResult>(_cacheContextAccessor));
            return (Cache<TKey, TResult>)result;
        }
    }
}
