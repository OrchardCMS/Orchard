using System;
using Orchard.Tasks.Locking;

namespace Orchard.Tests.Stubs {
    public class StubDistributedLock : IDistributedLock {
        public static bool IsAcquired { get; private set; }

        public bool IsDisposed { get; private set; }

        public bool TryAcquire(string name, TimeSpan maxLifetime) {
            if (IsAcquired)
                return false;

            return IsAcquired = true;
        }


        public void Dispose() {
            IsDisposed = true;
        }
    }
}
