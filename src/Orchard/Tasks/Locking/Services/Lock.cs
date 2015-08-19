using Orchard.Logging;

namespace Orchard.Tasks.Locking.Services {

    /// <summary>
    /// Provides a database driven implementation of <see cref="IDistributedLock" />
    /// </summary>
    public class Lock : IDistributedLock {
        private readonly IDistributedLockService _distributedLockService;
        public string Name { get; set; }
        private bool _isDisposed;

        public Lock(IDistributedLockService distributedLockService, string name, int id) {
            _distributedLockService = distributedLockService;
            Name = name;
            Id = id;
        }

        public ILogger Logger { get; set; }
        public int Id { get; set; }

        // This will be called at least and at the latest by the IoC container when the request ends.
        public void Dispose() {
            if (!_isDisposed) {
                _isDisposed = true;

                _distributedLockService.DisposeLock(this);
            }
        }
    }
}