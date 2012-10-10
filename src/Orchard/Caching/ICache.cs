using System;

namespace Orchard.Caching {
    public interface ICache<TKey, TResult> {
        TResult Get(TKey key, Func<AcquireContext<TKey>, TResult> acquire);
        bool IsValid(TKey key);
        bool TryGet(TKey key, out TResult value);
    }
}
