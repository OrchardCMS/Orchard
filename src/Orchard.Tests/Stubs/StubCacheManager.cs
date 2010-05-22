using System;
using Orchard.Caching;

namespace Orchard.Tests.Stubs {
    public class StubCacheManager : ICacheManager {
        public TResult Get<TKey, TResult>(TKey key, Func<AcquireContext<TKey>, TResult> acquire) {
            var cache = new Cache<TKey, TResult>();
            return cache.Get(key, acquire);
        }

        public ICache<TKey, TResult> GetCache<TKey, TResult>() {
            throw new NotImplementedException();
        }
    }
}