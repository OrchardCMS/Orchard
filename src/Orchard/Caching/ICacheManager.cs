using System;

namespace Orchard.Caching {
    public interface ICacheManager : ISingletonDependency {
        TResult Get<TKey, TResult>(TKey key, Func<AcquireContext<TKey>, TResult> acquire);
        ICache<TKey, TResult> GetCache<TKey, TResult>();
    }
}
