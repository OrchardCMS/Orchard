using System;
using Orchard.Caching.Providers;

namespace Orchard.Caching {
    public class AcquireContext<TKey> {
        public AcquireContext(TKey key, Action<IVolatileSignal> monitor) {
            Key = key;
            IsInvalid = monitor;
        }

        public TKey Key { get; private set; }
        public Action<IVolatileSignal> IsInvalid { get; private set; }
    }
}
