using System;
using System.Threading.Tasks;
using Orchard.Environment;
using Orchard.Logging;

namespace Orchard.Tasks.Locking {
    public class LockService : ILockService {
        private readonly Work<ILock> _lock;
        private readonly IMachineNameProvider _machineNameProvider;

        public LockService(Work<ILock> @lock, IMachineNameProvider machineNameProvider) {
            _lock = @lock;
            _machineNameProvider = machineNameProvider;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public ILock TryAcquireLock(string name, TimeSpan maxLifetime, TimeSpan timeout) {
            var waitedTime = TimeSpan.Zero;
            var waitTime = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / 10);
            var @lock = _lock.Value;
            bool acquired;

            while (!(acquired = @lock.TryAcquire(name, maxLifetime)) && waitedTime < timeout) {
                Task.Delay(timeout).ContinueWith(t => {
                    waitedTime += waitTime;
                }).Wait();
            }

            var machineName = _machineNameProvider.GetMachineName();

            if (acquired) {
                Logger.Debug(String.Format("Successfully acquired a lock named {0} on machine {1}.", name, machineName));
                return @lock;
            }

            Logger.Debug(String.Format("Failed to acquire a lock named {0} on machine {1}.", name, machineName));
            return null;
        }

        public ILock AcquireLock(string name, TimeSpan maxLifetime, TimeSpan timeout) {
            var lockResult = TryAcquireLock(name, maxLifetime, timeout);
            if (lockResult != null) return lockResult;
            throw new TimeoutException(String.Format("No lock for \"{0}\" could not be acquired within {1} milliseconds.", name, timeout.TotalMilliseconds));
        }
    }
}