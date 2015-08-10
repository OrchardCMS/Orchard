using System;

namespace Orchard.Tasks.Locking {
    public class DefaultLock : IDistributedLock {

        public bool TryAcquire(string name, TimeSpan maxLifetime) {
            return true;
        }

        public void Dispose() {
            // Noop.
        }
    }
}