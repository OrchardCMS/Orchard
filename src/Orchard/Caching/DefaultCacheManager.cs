using System;
using System.Collections.Generic;
using Castle.Core;

namespace Orchard.Caching {
    public class DefaultCacheManager : ICacheManager {
        private readonly Dictionary<Pair<Type, Type>, object> _caches;

        public DefaultCacheManager() {
            _caches = new Dictionary<Pair<Type, Type>, object>();
        }

        #region Implementation of ICacheManager

        public TResult Get<TKey, TResult>(TKey key, Func<AcquireContext<TKey>, TResult> acquire) {
            return GetCache<TKey, TResult>().Get(key, acquire);
        }

        public ICache<TKey, TResult> GetCache<TKey, TResult>() {
            var cacheKey = new Pair<Type, Type>(typeof(TKey), typeof(TResult));
            object value;
            if (!_caches.TryGetValue(cacheKey, out value)) {
                value = new Cache<TKey, TResult>();
                _caches.Add(cacheKey, value);
            }

            return (ICache<TKey, TResult>)value;
        }

        #endregion
    }
}
