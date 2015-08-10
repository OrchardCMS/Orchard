using System;
using System.Data;
using System.Linq;
using Autofac;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Exceptions;
using Orchard.Services;
using Orchard.TaskLease.Models;
using Orchard.Tasks.Locking;
using Orchard.Validation;

namespace Orchard.TaskLease.Services {

    /// <summary>
    /// Provides a database driven implementation of <see cref="ILock" />
    /// </summary>
    [OrchardSuppressDependency("Orchard.Tasks.Locking.DefaultLock")]
    public class DatabaseLock : ILock {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IClock _clock;
        private string _name;
        private bool _isAcquired;
        private int _id;
        private bool _isDisposed;

        public DatabaseLock(ILifetimeScope lifetimeScope, IClock clock) {
            _lifetimeScope = lifetimeScope;
            _clock = clock;
        }

        public bool TryAcquire(string name, TimeSpan maxLifetime) {
            Argument.ThrowIfNullOrEmpty(name, "name");

            if (name.Length > 256)
                throw new ArgumentException("The lock's name can't be longer than 256 characters.");

            // This way we can create a nested transaction scope instead of having the unwanted effect
            // of manipulating the transaction of the caller.
            using (var scope = BeginLifeTimeScope(name)) {
                var repository = scope.Resolve<IRepository<DatabaseLockRecord>>();
                var record = repository.Table.FirstOrDefault(x => x.Name == name);

                if (record != null) {
                    // There is a nexisting lock, but check if it has expired.
                    var isExpired = record.AcquiredUtc + maxLifetime < _clock.UtcNow;
                    if (isExpired) {
                        repository.Delete(record);
                        record = null;
                    }
                }

                var canAcquire = record == null;

                if (canAcquire) {
                    record = new DatabaseLockRecord {
                        Name = name,
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
        }

        // This will be called at least by the IoC container when the request ends.
        public void Dispose() {
            if (_isDisposed || !_isAcquired) return;

            _isDisposed = true;

            using (var scope = BeginLifeTimeScope(_name)) {
                try {
                    var repository = scope.Resolve<IRepository<DatabaseLockRecord>>();
                    var record = repository.Get(_id);

                    if (record != null) {
                        repository.Delete(record);
                        repository.Flush();
                    }
                }
                catch (Exception ex) {
                    if (ex.IsFatal()) throw;
                }
            }
        }

        private ILifetimeScope BeginLifeTimeScope(string name) {
            var scope = _lifetimeScope.BeginLifetimeScope("Orchard.Tasks.Locking.Database." + name);
            scope.Resolve<ITransactionManager>().RequireNew(IsolationLevel.ReadCommitted);
            return scope;
        }
    }
}