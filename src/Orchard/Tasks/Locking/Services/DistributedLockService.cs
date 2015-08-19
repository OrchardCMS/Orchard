using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Exceptions;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Tasks.Locking.Records;

namespace Orchard.Tasks.Locking.Services {

    public class DistributedLockService : IDistributedLockService {
        private readonly IMachineNameProvider _machineNameProvider;
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IClock _clock;
        private readonly object _semaphore = new object();

        public DistributedLockService(IMachineNameProvider machineNameProvider, ILifetimeScope lifetimeScope, IClock clock) {
            _machineNameProvider = machineNameProvider;
            _lifetimeScope = lifetimeScope;
            _clock = clock;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool TryAcquireLock(string name, TimeSpan maxLifetime, TimeSpan timeout, out IDistributedLock @lock) {
            lock (_semaphore) {
                @lock = default(IDistributedLock);

                try {
                    var waitedTime = TimeSpan.Zero;
                    var waitTime = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / 10);
                    bool acquired;

                    while (!(acquired = TryAcquireLockRecord(name, maxLifetime, out @lock)) && waitedTime < timeout) {
                        Task.Delay(timeout).ContinueWith(t => {
                            waitedTime += waitTime;
                        }).Wait();
                    }

                    if (acquired) {
                        Logger.Debug("Successfully acquired a lock named {0}.", name);
                        return true;
                    }
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error while trying to acquire a lock named {0}.", name);
                    throw;
                }

                Logger.Debug("Could not acquire a lock named {0}.", name);
                return false;
            }
        }

        public IDistributedLock AcquireLock(string name, TimeSpan maxLifetime, TimeSpan timeout) {
            IDistributedLock lockResult;
            return TryAcquireLock(name, maxLifetime, timeout, out lockResult) ? lockResult : null;
        }

        public void DisposeLock(IDistributedLock @lock) {
            var childLifetimeScope = CreateChildLifetimeScope(@lock.Name);

            try {
                var repository = childLifetimeScope.Resolve<IRepository<LockRecord>>();
                var transactionManager = childLifetimeScope.Resolve<ITransactionManager>();
                transactionManager.RequireNew(IsolationLevel.ReadCommitted);
                var record = repository.Get(@lock.Id);

                if (record != null) {
                    if (record.ReferenceCount > 0)
                        record.ReferenceCount--;
                }
            }
            catch (Exception ex) {
                if (ex.IsFatal()) throw;
                Logger.Error(ex, "An non-fatal error occurred while trying to dispose a distributed lock with name {0} and ID {1}.", @lock.Name, @lock.Id);
            }
            finally {
                childLifetimeScope.Dispose();
            }
        }

        protected virtual bool TryAcquireLockRecord(string name, TimeSpan maxLifetime, out IDistributedLock @lock) {
            @lock = null;
            var childLifetimeScope = CreateChildLifetimeScope(name);

            try {
                var transactionManager = childLifetimeScope.Resolve<ITransactionManager>();
                transactionManager.RequireNew(IsolationLevel.ReadCommitted);

                // This way we can create a nested transaction scope instead of having the unwanted effect
                // of manipulating the transaction of the caller.
                var repository = childLifetimeScope.Resolve<IRepository<LockRecord>>();

                // Find an existing, active lock, if any.
                var record = repository.Table.FirstOrDefault(x => x.Name == name && (x.ValidUntilUtc >= _clock.UtcNow || x.ReferenceCount > 0));

                // The current owner name (based on machine name and current thread ID).
                var currentOwnerName = GetOwnerName();
                var canAcquireLock = false;

                // Check if there's already an active lock.
                if (record != null) {
                    // Check if the owner of the lock is the one trying to acquire the lock.
                    if (record.Owner == currentOwnerName) {
                        record.ReferenceCount++;
                        canAcquireLock = true;
                    }
                }
                else {
                    // No one has an active lock yet, so good to go.
                    record = new LockRecord {
                        Name = name,
                        Owner = currentOwnerName,
                        ReferenceCount = 1,
                        CreatedUtc = _clock.UtcNow,
                        ValidUntilUtc = _clock.UtcNow + maxLifetime
                    };
                    repository.Create(record);
                    repository.Flush();
                    canAcquireLock = true;
                }

                if (!canAcquireLock)
                    return false;

                @lock = new Lock(this, name, record.Id);
                return true;
            }
            catch (Exception ex) {
                Logger.Error(ex, "An error occurred while trying to acquire a lock.");
                throw;
            }
            finally {
                childLifetimeScope.Dispose();
            }
        }

        private string GetOwnerName() {
            return String.Format("{0}_{1}", _machineNameProvider.GetMachineName(), Thread.CurrentThread.ManagedThreadId);
        }

        private ILifetimeScope CreateChildLifetimeScope(string name) {
            return _lifetimeScope.BeginLifetimeScope("Orchard.Tasks.Locking." + name);
        }
    }
}