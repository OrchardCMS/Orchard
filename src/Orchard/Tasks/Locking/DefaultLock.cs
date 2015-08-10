using System;

namespace Orchard.Tasks.Locking {

    public class DefaultLock : IDistributedLock {
        private readonly IStaticLockSemaphore _semaphore;

        public DefaultLock(IStaticLockSemaphore semaphore) {
            _semaphore = semaphore;
        }

        public bool TryAcquire(string name, TimeSpan maxLifetime) {
            if (_semaphore.IsAcquired)
                return false;

            return _semaphore.IsAcquired = true;
        }

        public void Dispose() {
            _semaphore.IsAcquired = false;
        }

        public interface IStaticLockSemaphore : ISingletonDependency {
            bool IsAcquired { get; set; }
        }

        public class StaticLockSemaphore : IStaticLockSemaphore {
            public bool IsAcquired { get; set; }
        }
    }
}