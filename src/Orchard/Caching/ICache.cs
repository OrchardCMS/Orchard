using System;

namespace Orchard.Caching {
    public interface ICache<TKey, TResult> {
        TResult Get(TKey key, Func<AcquireContext, TResult> acquire);
    }
}
