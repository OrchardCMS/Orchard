using System;
using System.Threading;

namespace Orchard.Tasks.Locking.Services {

    /// <summary>
    /// Represents a distributed lock. />
    /// </summary>
    public class DistributedLock : IDisposable {
        private IDistributedLockService _service;
        private int _isDisposed;

        private DistributedLock() {
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string MachineName { get; private set; }
        public int? ThreadId { get; private set; }

        // This will be called at least and at the latest by the IoC container when the request ends.
        public void Dispose() {
            if(Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
                _service.ReleaseLock(this);
        }

        public static DistributedLock ForMachine(IDistributedLockService service, string name, string machineName, string lockId) {
            return new DistributedLock {
                _service = service,
                Name = name,
                MachineName = machineName,
                Id = lockId
            };
        }

        public static DistributedLock ForThread(IDistributedLockService service, string name, string machineName, int threadId, string lockId) {
            return new DistributedLock {
                _service = service,
                Name = name,
                MachineName = machineName,
                ThreadId = threadId,
                Id = lockId
            };
        }
    }
}