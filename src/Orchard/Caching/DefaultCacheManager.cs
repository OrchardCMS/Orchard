using System;

namespace Orchard.Caching {
    /// <summary>
    /// Provides the default implementation for a cache manager. 
    /// The cache manager provides an abstraction over the cache holder allowing it to be easily swaped and isolating it within a component context.
    /// </summary>
    public class DefaultCacheManager : ICacheManager {
        private readonly Type _component;
        private readonly ICacheHolder _cacheHolder;

        /// <summary>
        /// Constructs a new cache manager for a given component type and with a specific cache holder implementation.
        /// </summary>
        /// <param name="component">The component to which the cache applies (context).</param>
        /// <param name="cacheHolder">The cache holder that contains the entities cached.</param>
        public DefaultCacheManager(Type component, ICacheHolder cacheHolder) {
            _component = component;
            _cacheHolder = cacheHolder;
        }

        /// <summary>
        /// Gets a cache entry from the cache holder.
        /// </summary>
        /// <typeparam name="TKey">The type of the key to be used to fetch the cache entry.</typeparam>
        /// <typeparam name="TResult">The type of the entry to be obtained from the cache.</typeparam>
        /// <returns>The entry from the cache.</returns>
        public ICache<TKey, TResult> GetCache<TKey, TResult>() {
            return _cacheHolder.GetCache<TKey, TResult>(_component);
        }

        public TResult Get<TKey, TResult>(TKey key, Func<AcquireContext<TKey>, TResult> acquire) {
            return GetCache<TKey, TResult>().Get(key, acquire);
        }
    }
}
