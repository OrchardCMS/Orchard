using System;
using System.Threading;
using System.Threading.Tasks;
using Orchard.Environment;
using Orchard.Logging;

namespace Orchard.Tasks.Locking {
    public class DistributedLockService : IDistributedLockService {
        private readonly ReaderWriterLockSlim _rwl = new ReaderWriterLockSlim();
        private readonly IWorkContextAccessor _wca;
        private readonly IMachineNameProvider _machineNameProvider;

        public DistributedLockService(IWorkContextAccessor wca, IMachineNameProvider machineNameProvider) {
            _wca = wca;
            _machineNameProvider = machineNameProvider;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool TryAcquireLock(string name, TimeSpan maxLifetime, TimeSpan timeout, out IDistributedLock @lock) {
            var machineName = _machineNameProvider.GetMachineName();
            @lock = Resolve<IDistributedLock>();

            if (_rwl.TryEnterWriteLock(0)) {
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
                        Logger.Debug("Successfully acquired a lock named {0} on machine {1}.", name, machineName);
                        return true;
                    }
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error while trying to acquire a lock named {0} on machine {1}.", name, machineName);
                    throw;
                }
                finally {
                    _rwl.ExitWriteLock();
                }
            }

            Logger.Debug("Could not acquire a lock named {0} on machine {1}.", name, machineName);
            return false;
        }

        private T Resolve<T>() {
            var workContext = _wca.GetContext() ?? _wca.CreateWorkContextScope().WorkContext; // In case this is invoked at the end of the request.
            return workContext.Resolve<T>();
        }

        public IDistributedLock AcquireLock(string name, TimeSpan maxLifetime, TimeSpan timeout) {
            IDistributedLock lockResult;
            return TryAcquireLock(name, maxLifetime, timeout, out lockResult) ? lockResult : null;
        }
    }
}