using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Tasks.Locking.Providers;
using Orchard.Tasks.Locking.Records;

namespace Orchard.Tests.Tasks {
    [TestFixture]
    public class DatabaseLockTests : DatabaseEnabledTestsBase {
        private const string MachineName1 = "Orchard Testing Machine 1";
        private const string MachineName2 = "Orchard Testing Machine 2";
        private const string LockName = "Orchard Test Lock";
        private IRepository<DatabaseLockRecord> _databaseLockRepository;
        private DatabaseLock _lock;

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                yield return typeof(DatabaseLockRecord);
            }
        }

        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<DatabaseLock>().AsSelf();
        }

        public override void Init() {
            base.Init();

            _databaseLockRepository = _container.Resolve<IRepository<DatabaseLockRecord>>();
            _lock = _container.Resolve<DatabaseLock>();
        }

        [Test]
        public void AcquiringLockSucceeds() {
            var lockAcquired = _lock.TryAcquire(LockName, MachineName1, TimeSpan.FromSeconds(60));

            Assert.That(lockAcquired, Is.True);
            Assert.That(_databaseLockRepository.Table.Count(), Is.EqualTo(1));
        }

        [Test]
        public void DisposingRemovesLockRecord() {
            _lock.TryAcquire(LockName, MachineName1, TimeSpan.FromSeconds(60));
            _lock.Dispose();
            Assert.That(_databaseLockRepository.Table.Count(), Is.EqualTo(0));
        }

        [Test]
        public void AcquiringLockTwiceFails() {
            var attempt1 = _lock.TryAcquire(LockName, MachineName1, TimeSpan.FromSeconds(60));
            var attempt2 = _lock.TryAcquire(LockName, MachineName2, TimeSpan.FromSeconds(60));
            
            Assert.That(attempt1, Is.True);
            Assert.That(attempt2, Is.False);
        }

        [Test]
        public void AcquiringExpiredLockSucceeds() {
            var attempt1 = _lock.TryAcquire(LockName, MachineName1, TimeSpan.FromSeconds(60));
            var attempt2 = _lock.TryAcquire(LockName, MachineName2, TimeSpan.FromSeconds(-1)); // Treat the previosuly stored lock as immediately expired.

            Assert.That(attempt1, Is.True);
            Assert.That(attempt2, Is.True);
        }
    }
}