using System;
using System.Threading.Tasks;
using Orchard.Environment;
using Orchard.Logging;

namespace Orchard.Tasks.Locking {
    public class DistributedLockService : IDistributedLockService {
        private readonly Work<IDistributedLock> _lock;
        private readonly IMachineNameProvider _machineNameProvider;

        public DistributedLockService(Work<IDistributedLock> @lock, IMachineNameProvider machineNameProvider) {
            _lock = @lock;
            _machineNameProvider = machineNameProvider;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool TryAcquireLock(string name, TimeSpan maxLifetime, TimeSpan timeout, out IDistributedLock @lock) {
            var waitedTime = TimeSpan.Zero;
            var waitTime = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / 10);
            @lock = _lock.Value;
            bool acquired;

            while (!(acquired = @lock.TryAcquire(name, maxLifetime)) && waitedTime < timeout) {
                Task.Delay(timeout).ContinueWith(t => {
                    waitedTime += waitTime;
                }).Wait();
            }

            var machineName = _machineNameProvider.GetMachineName();

            if (acquired) {
                Logger.Debug(String.Format("Successfully acquired a lock named {0} on machine {1}.", name, machineName));
                return true;
            }

            Logger.Debug(String.Format("Failed to acquire a lock named {0} on machine {1}.", name, machineName));
            return false;
        }

        public IDistributedLock AcquireLock(string name, TimeSpan maxLifetime, TimeSpan timeout) {
            IDistributedLock lockResult;
            return TryAcquireLock(name, maxLifetime, timeout, out lockResult) ? lockResult : null;
        }
    }
}