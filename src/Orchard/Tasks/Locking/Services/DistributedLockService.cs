using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Exceptions;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Tasks.Locking.Records;

namespace Orchard.Tasks.Locking.Services {

    public class DistributedLockService : Component, IDistributedLockService {
        private readonly IClock _clock;
        private readonly IMachineNameProvider _machineNameProvider;
        private readonly ShellSettings _shellSettings;
        private readonly ILifetimeScope _lifetimeScope;
        private readonly ConcurrentDictionary<string, DistributedLock> _locks = new ConcurrentDictionary<string, DistributedLock>();

        public bool DisableMonitorLock { get; set; }

        public DistributedLockService(
            IMachineNameProvider machineNameProvider,
            ILifetimeScope lifetimeScope,
            IClock clock,
            ShellSettings shellSettings) {
            _clock = clock;
            _lifetimeScope = lifetimeScope;

            _shellSettings = shellSettings;
            _machineNameProvider = machineNameProvider;
        }

        public bool TryAcquireLock(string name, TimeSpan? maxValidFor, TimeSpan? timeout, out IDistributedLock l) {
            try {
                l = AcquireLock(name, maxValidFor, timeout);
                return l != null;
            }
            catch {
                l = null;
                return false;
            }
        }

        public IDistributedLock AcquireLock(string name, TimeSpan? maxValidFor, TimeSpan? timeout) {
            DistributedLock l = null;
            
            try {

                var acquired = Poll(() => (l = AcquireLockInternal(name, maxValidFor)) != null, timeout);

                if (acquired) {
                    Logger.Debug("Successfully acquired a lock named '{0}'.", name);
                }
                else {
                    Logger.Debug("Failed to acquire a lock named '{0}'.", name);
                }
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error while trying to acquire a lock named '{0}'.", name);
                throw;
            }

            if (l == null && timeout != null) {
                throw new TimeoutException(String.Format("Failed to acquire a lock named '{0}' within the specified timeout ('{1}').", name, timeout));
            }

            return l;
        }

        public void ReleaseDistributedLock(DistributedLock l) {

            try {
                var record = GetDistributedLockRecordByName(l.Name);

                if (record == null) {
                    throw new OrchardException(T("No lock record could be found for the specified lock to be released."));
                }

                TryCommitNewTransaction(repository => repository.Delete(record));
            }
            catch (Exception ex) {
                if (ex.IsFatal()) throw;
                Logger.Error(ex, "An non-fatal error occurred while trying to dispose a distributed lock with name '{0}'.", l.Name);
            }
        }

        private DistributedLockRecord GetDistributedLockRecordByName(string name) {
            DistributedLockRecord result = null;
            TryCommitNewTransaction(repository => {
                result = repository.Table.FirstOrDefault(x =>
                    x.Name == name
                );
            });

            return result;
        }

        private DistributedLockRecord GetValidDistributedLockRecordByName(string name) {
            DistributedLockRecord result = null;
            TryCommitNewTransaction(repository => {
                result = repository.Table.FirstOrDefault(x =>
                    x.Name == name &&
                    (x.ValidUntilUtc == null || x.ValidUntilUtc >= _clock.UtcNow)
                );
            });

            return result;            
        }
        
        private DistributedLock AcquireLockInternal(string name, TimeSpan? maxValidFor) {
            try {
                name = GetTenantLockName(name);

                if (!DisableMonitorLock && !Monitor.TryEnter(String.Intern(name))) {
                    return null;
                }

                DistributedLock dLock = null;
                
                // Returns the existing lock in case of reentrancy.
                if(!DisableMonitorLock && _locks.TryGetValue(name, out dLock)) {
                    dLock.IncreaseReferenceCount();
                    return dLock;
                }

                // Find an existing, active lock, if any.
                var record = GetValidDistributedLockRecordByName(name);

                // The current owner name (based on machine name).
                var canAcquireLock = false;

                // Check if there's already an active lock.
                if (record != null) {
                    // Check if the machine name assigned to the lock is the one trying to acquire it.
                    if (record.MachineName == _machineNameProvider.GetMachineName()) {
                        canAcquireLock = true;
                    }
                }
                else {
                    // No one has an active lock yet, so good to go.
                    record = new DistributedLockRecord {
                        Name = name,
                        MachineName = _machineNameProvider.GetMachineName(),
                        CreatedUtc = _clock.UtcNow,
                        ValidUntilUtc = maxValidFor != null ? _clock.UtcNow + maxValidFor : null
                    };

                    
                    canAcquireLock = TryCommitNewTransaction( repository => {
                        repository.Create(record);
                    });
                }

                if (!canAcquireLock) {
                    return null;
                }

                dLock = new DistributedLock(this, name);
                if (!DisableMonitorLock) {
                    _locks.TryAdd(name, dLock);
                }

                return dLock;
            }
            catch (Exception ex) {
                Monitor.Exit(String.Intern(name));

                Logger.Error(ex, "An error occurred while trying to acquire a lock.");
                throw;
            }           
        }

        private string GetTenantLockName(string name) {
            return _shellSettings.Name + ":" + name;
        }

        /// <summary>
        /// Executes the specified function until it returns true, for the specified amount of time, or indefinitely if no timeout was given.
        /// </summary>
        /// <param name="operation">The operation to repeatedly execute until it returns true.</param>
        /// <param name="timeout">The amount of time to retry executing the function. If null is specified, the specified function is executed indefinitely until it returns true.</param>
        /// <returns>Returns true if the specified function returned true within the specified timeout, false otherwise.</returns>
        private bool Poll(Func<bool> operation, TimeSpan? timeout) {
            var waitedTime = TimeSpan.Zero;
            var waitTime = TimeSpan.FromMilliseconds(timeout.GetValueOrDefault().TotalMilliseconds / 10);
            bool acquired;

            while (!(acquired = operation()) && (timeout == null || waitedTime < timeout.Value)) {
                Task.Delay(waitTime).ContinueWith(t => {
                    waitedTime += waitTime;
                }).Wait();
            }

            return acquired;
        }

        private bool TryCommitNewTransaction(Action<IRepository<DistributedLockRecord>> action) {
            if (action == null) {
                throw new ArgumentNullException();
            }

            try {
                using (var childLifetimeScope = _lifetimeScope.BeginLifetimeScope()) {
                    var repository = childLifetimeScope.Resolve<IRepository<DistributedLockRecord>>();
                    var transactionManager = childLifetimeScope.Resolve<ITransactionManager>();
                    transactionManager.RequireNew(IsolationLevel.ReadCommitted);
                    action(repository);
                }

                return true;
            }
            catch {
                return false;
            }
            
        }
    }
}