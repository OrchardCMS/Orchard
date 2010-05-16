using System;

namespace Orchard.Caching {
    public class DefaultCacheManager : ICacheManager {
        private readonly Type _component;
        private readonly ICacheHolder _cacheHolder;

        public DefaultCacheManager(Type component, ICacheHolder cacheHolder) {
            _component = component;
            _cacheHolder = cacheHolder;
        }

        public ICache<TKey, TResult> GetCache<TKey, TResult>() {
            return _cacheHolder.GetCache<TKey, TResult>(_component);
        }

        public TResult Get<TKey, TResult>(TKey key, Func<AcquireContext<TKey>, TResult> acquire) {
            return GetCache<TKey, TResult>().Get(key, acquire);
        }
    }
}
