using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Exceptions;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Tasks.Locking.Records;

namespace Orchard.Tasks.Locking.Services {

    public class DistributedLockService : Component, IDistributedLockService {
        private readonly IMachineNameProvider _machineNameProvider;
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IClock _clock;
        private readonly IThreadProvider _threadProvider;

        public DistributedLockService(IMachineNameProvider machineNameProvider, IThreadProvider threadProvider, ILifetimeScope lifetimeScope, IClock clock) {
            _machineNameProvider = machineNameProvider;
            _lifetimeScope = lifetimeScope;
            _clock = clock;
            _threadProvider = threadProvider;
        }

        public bool TryAcquireLockForMachine(string name, TimeSpan maxValidFor, TimeSpan? timeout, out DistributedLock @lock) {
            return TryAcquireLock(name, maxValidFor, timeout, GetMachineName(), null, out @lock);
        }

        public DistributedLock AcquireLockForMachine(string name, TimeSpan maxValidFor, TimeSpan? timeout) {
            return AcquireLock(name, maxValidFor, timeout, GetMachineName(), null);
        }

        public bool TryAcquireLockForThread(string name, TimeSpan maxValidFor, TimeSpan? timeout, out DistributedLock @lock) {
            return TryAcquireLock(name, maxValidFor, timeout, GetMachineName(), GetThreadId(), out @lock);
        }

        public DistributedLock AcquireLockForThread(string name, TimeSpan maxValidFor, TimeSpan? timeout) {
            return AcquireLock(name, maxValidFor, timeout, GetMachineName(), GetThreadId());
        }

        public void ReleaseLock(DistributedLock @lock) {
            var childLifetimeScope = CreateChildLifetimeScope(@lock.Name);

            try {
                var repository = childLifetimeScope.Resolve<IRepository<DistributedLockRecord>>();
                var transactionManager = childLifetimeScope.Resolve<ITransactionManager>();
                transactionManager.RequireNew(IsolationLevel.ReadCommitted);
                var lockId = Int32.Parse(@lock.Id);
                var record = repository.Get(lockId);

                if (record == null)
                    throw new ObjectDisposedException("@lock", "No lock record could be found for the specified lock to be released.");

                if (record.Count <= 0)
                    throw new ObjectDisposedException("@lock", "The specified lock has already been released.");

                record.Count--;
            }
            catch (Exception ex) {
                if (ex.IsFatal()) throw;
                Logger.Error(ex, "An non-fatal error occurred while trying to dispose a distributed lock with name '{0}' and ID {1}.", @lock.Name, @lock.Id);
            }
            finally {
                childLifetimeScope.Dispose();
            }
        }

        private bool TryAcquireLock(string name, TimeSpan maxValidFor, TimeSpan? timeout, string machineName, int? threadId, out DistributedLock @lock) {
            @lock = AcquireLockInternal(name, maxValidFor, machineName, threadId, timeout ?? TimeSpan.Zero);
            if (@lock != null)
                return true;

            return false;
        }

        private DistributedLock AcquireLock(string name, TimeSpan maxValidFor, TimeSpan? timeout, string machineName, int? threadId) {
            var @lock = AcquireLockInternal(name, maxValidFor, machineName, threadId, timeout);
            if (@lock != null)
                return @lock;

            throw new TimeoutException(String.Format("Failed to acquire a lock named '{0}' within the specified timeout ('{1}').", name, timeout));
        }

        private DistributedLock AcquireLockInternal(string name, TimeSpan maxValidFor, string machineName, int? threadId, TimeSpan? timeout = null) {
            try {
                DistributedLockRecord record = null;
                var acquired = Poll(() => (record = AcquireLockRecord(name, maxValidFor, machineName, threadId)) != null, timeout);

                if (acquired) {
                    Logger.Debug("Successfully acquired a lock named '{0}'.", name);
                    return threadId != null
                        ? DistributedLock.ForThread(this, name, machineName, threadId.Value, record.Id.ToString())
                        : DistributedLock.ForMachine(this, name, machineName, record.Id.ToString());
                }
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error while trying to acquire a lock named '{0}'.", name);
                throw;
            }

            Logger.Debug(timeout == null
                ? "Failed to acquire a lock named '{0}'."
                : "Failed to acquire a lock named '{0}' within the specified timeout ('{1}')."
                , name, timeout);

            return null;
        }

        private DistributedLockRecord AcquireLockRecord(string name, TimeSpan maxValidFor, string machineName, int? threadId) {
            //lock (_transactionManagerLock) {
                var childLifetimeScope = CreateChildLifetimeScope(name);

                try {
                    var transactionManager = childLifetimeScope.Resolve<ITransactionManager>();
                    transactionManager.RequireNew(IsolationLevel.ReadCommitted);

                    // This way we can create a nested transaction scope instead of having the unwanted effect
                    // of manipulating the transaction of the caller.
                    var repository = childLifetimeScope.Resolve<IRepository<DistributedLockRecord>>();

                    // Find an existing, active lock, if any.
                    var record = repository.Table.FirstOrDefault(x => x.Name == name && x.ValidUntilUtc >= _clock.UtcNow && x.Count > 0);

                    // The current owner name (based on machine name and current thread ID).
                    var canAcquireLock = false;

                    // Check if there's already an active lock.
                    if (record != null) {
                        // Check if the machine name assigned to the lock is the one trying to acquire it.
                        if (record.MachineName == machineName) {
                            if (record.ThreadId != threadId)
                                throw new InvalidOperationException(
                                    threadId == null
                                    ? "An attempt to acquire a lock for a machine was detected while the requested lock is already assigned to a specific thread."
                                    : "An attempt to acquire a lock for a thread was detected while the requested lock is already assigned to a machine.");

                            record.Count++;
                            canAcquireLock = true;
                        }
                    }
                    else {
                        // No one has an active lock yet, so good to go.
                        record = new DistributedLockRecord {
                            Name = name,
                            MachineName = machineName,
                            ThreadId = threadId,
                            Count = 1,
                            CreatedUtc = _clock.UtcNow,
                            ValidUntilUtc = _clock.UtcNow + maxValidFor
                        };
                        repository.Create(record);
                        canAcquireLock = true;
                    }

                    if (!canAcquireLock)
                        return null;

                    return record;
                }
                catch (Exception ex) {
                    Logger.Error(ex, "An error occurred while trying to acquire a lock.");
                    throw;
                }
                finally {
                    childLifetimeScope.Dispose();
                }
            //}
        }

        /// <summary>
        /// Executes the specified function until it returns true, for the specified amount of time, or indefinitely if no timeout was given.
        /// </summary>
        /// <param name="pollFunc">The function to repeatedly execute until it returns true.</param>
        /// <param name="timeout">The amount of time to retry executing the function. If null is specified, the specified function is executed indefinitely until it returns true.</param>
        /// <returns>Returns true if the specified function returned true within the specified timeout, false otherwise.</returns>
        private bool Poll(Func<bool> pollFunc, TimeSpan? timeout) {
            var waitedTime = TimeSpan.Zero;
            var waitTime = TimeSpan.FromMilliseconds(timeout.GetValueOrDefault().TotalMilliseconds / 10);
            bool acquired;

            while (!(acquired = pollFunc()) && (timeout == null || waitedTime < timeout.Value)) {
                Task.Delay(waitTime).ContinueWith(t => {
                    waitedTime += waitTime;
                }).Wait();
            }

            return acquired;
        }

        private string GetMachineName() {
            return _machineNameProvider.GetMachineName();
        }

        private int GetThreadId() {
            return _threadProvider.GetCurrentThreadId();
        }

        private ILifetimeScope CreateChildLifetimeScope(string name) {
            return _lifetimeScope.BeginLifetimeScope("Orchard.Tasks.Locking." + name);
        }
    }
}