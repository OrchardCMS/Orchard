using System;
using Orchard.Caching;

namespace Orchard.Tests.Stubs {
    public class StubCacheManager : ICacheManager {
        private readonly ICacheManager _defaultCacheManager;

        public StubCacheManager() {
            _defaultCacheManager = new DefaultCacheManager(this.GetType(), new DefaultCacheHolder());
        }
        public TResult Get<TKey, TResult>(TKey key, Func<AcquireContext<TKey>, TResult> acquire) {
            return _defaultCacheManager.Get(key, acquire);
        }

        public ICache<TKey, TResult> GetCache<TKey, TResult>() {
            return _defaultCacheManager.GetCache<TKey, TResult>();
        }
    }
}
