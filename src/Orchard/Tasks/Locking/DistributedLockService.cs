using System;
using System.Threading;
using System.Threading.Tasks;
using Orchard.Environment;
using Orchard.Logging;

namespace Orchard.Tasks.Locking {
    public class DistributedLockService : IDistributedLockService {
        private static readonly object _semaphore = new object();
        private readonly Work<IDistributedLock> _lock;
        private readonly IMachineNameProvider _machineNameProvider;

        public DistributedLockService(Work<IDistributedLock> @lock, IMachineNameProvider machineNameProvider) {
            _lock = @lock;
            _machineNameProvider = machineNameProvider;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool TryAcquireLock(string name, TimeSpan maxLifetime, TimeSpan timeout, out IDistributedLock @lock) {
            var machineName = _machineNameProvider.GetMachineName();
            @lock = _lock.Value;

            if (Monitor.TryEnter(_semaphore)) {
                try {
                    var waitedTime = TimeSpan.Zero;
                    var waitTime = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / 10);
                    bool acquired;

                    while (!(acquired = @lock.TryAcquire(name, maxLifetime)) && waitedTime < timeout) {
                        Task.Delay(timeout).ContinueWith(t => {
                            waitedTime += waitTime;
                        }).Wait();
                    }


                    if (acquired) {
                        Logger.Debug(String.Format("Successfully acquired a lock named {0} on machine {1}.", name, machineName));
                        return true;
                    }
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error during acquire lock.");
                    throw;
                }
                finally {
                    Monitor.Exit(_semaphore);
                    Logger.Debug("Ending sweep.");
                }
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