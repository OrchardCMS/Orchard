using System;
using System.Data;
using System.Linq;
using Autofac;
using Orchard.Data;
using Orchard.Exceptions;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Tasks.Locking.Records;
using Orchard.Tasks.Locking.Services;
using Orchard.Validation;

namespace Orchard.Tasks.Locking.Providers {

    /// <summary>
    /// Provides a database driven implementation of <see cref="IDistributedLock" />
    /// </summary>
    public class DatabaseLock : IDistributedLock {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IClock _clock;
        private string _name;
        private bool _isAcquired;
        private int _id;
        private bool _isDisposed;
        private ILifetimeScope _scope;

        public DatabaseLock(ILifetimeScope lifetimeScope, IClock clock) {
            _lifetimeScope = lifetimeScope;
            _clock = clock;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool TryAcquire(string name, string machineName, TimeSpan maxLifetime) {
            if(_isDisposed)
                throw new ObjectDisposedException("DatabaseLock");

            Argument.ThrowIfNullOrEmpty(name, "name");

            if (name.Length > 256)
                throw new ArgumentException("The lock's name can't be longer than 256 characters.");

            try {
                var scope = EnsureLifetimeScope(name);
                scope.Resolve<ITransactionManager>().RequireNew(IsolationLevel.ReadCommitted);

                // This way we can create a nested transaction scope instead of having the unwanted effect
                // of manipulating the transaction of the caller.
                var repository = scope.Resolve<IRepository<DatabaseLockRecord>>();
                var record = repository.Table.FirstOrDefault(x => x.Name == name);

                if (record != null) {
                    // There is an existing lock, but check if it has expired or if the current machine is requesting the lock.
                    var isExpired = record.AcquiredUtc + maxLifetime < _clock.UtcNow;
                    var isCurrentMachine = record.MachineName == machineName;
                    if (isExpired || isCurrentMachine) {
                        repository.Delete(record);
                        record = null;
                    }
                }

                var canAcquire = record == null;

                if (canAcquire) {
                    record = new DatabaseLockRecord {
                        Name = name,
                        MachineName = machineName,
                        AcquiredUtc = _clock.UtcNow
                    };
                    repository.Create(record);
                    repository.Flush();

                    _name = name;
                    _isAcquired = true;
                    _id = record.Id;
                }

                return canAcquire;
            }
            catch (Exception ex) {
                Logger.Error(ex, "An error occurred while trying to acquire a lock.");
                DisposeLifetimeScope();
                throw;
            }
        }

        // This will be called at least and at the latest by the IoC container when the request ends.
        public void Dispose() {
            if (_scope == null)
                return;

            if (!_isDisposed) {
                _isDisposed = true;

                if (_isAcquired) {
                    try {
                        var repository = _scope.Resolve<IRepository<DatabaseLockRecord>>();
                        var record = repository.Get(_id);

                        if (record != null) {
                            repository.Delete(record);
                            repository.Flush();
                        }
                    }
                    catch (Exception ex) {
                        if (ex.IsFatal()) throw;
                        Logger.Error(ex, "An non-fatal error occurred while trying to dispose the database lock.");
                    }
                }

                _scope.Dispose();
            }
        }

        private ILifetimeScope EnsureLifetimeScope(string name) {
            return _scope ?? (_scope = _lifetimeScope.BeginLifetimeScope("Orchard.Tasks.Locking.Database." + name));
        }

        private void DisposeLifetimeScope() {
            if (_scope != null)
                _scope.Dispose();
        }
    }
}