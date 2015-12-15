using System;

namespace Orchard.Caching {
    public interface ICacheHolder : ISingletonDependency {
        ICache<TKey, TResult> GetCache<TKey, TResult>(Type component);
    }
}
