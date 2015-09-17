using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Services;
using Orchard.Tasks.Locking.Records;
using Orchard.Tasks.Locking.Services;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Tasks {
    [TestFixture]
    public class DistributedLockServiceTests : DatabaseEnabledTestsBase {
        private const string LockName = "Orchard Test Lock";
        private DistributedLockService _distributedLockService;
        private StubApplicationEnvironment _applicationEnvironment;
        private IRepository<DistributedLockRecord> _distributedLockRepository;
        private ITransactionManager _transactionManager;

        protected override IEnumerable<Type> DatabaseTypes {
            get { yield return typeof(DistributedLockRecord); }
        }

        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<StubClock>().As<IClock>();
            builder.RegisterType<StubApplicationEnvironment>().As<IApplicationEnvironment>().SingleInstance();
            builder.RegisterType<DistributedLockService>().AsSelf();
        }

        public override void Init() {
            base.Init();
            _distributedLockService = _container.Resolve<DistributedLockService>();
            _applicationEnvironment = (StubApplicationEnvironment)_container.Resolve<IApplicationEnvironment>();
            _distributedLockRepository = _container.Resolve<IRepository<DistributedLockRecord>>();
            _transactionManager = _container.Resolve<ITransactionManager>();
        }

        [Test]
        public void TryAcquiringLockTwiceOnSameMachineSucceeds() {
            IDistributedLock @lock;
            var attempt1 = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), out @lock);
            var attempt2 = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), out @lock);

            Assert.That(attempt1, Is.True);
            Assert.That(attempt2, Is.True);
        }


        [Test]
        public void AcquiringTheLockOnTheSameMachineReturnsTheSameLock() {
            IDistributedLock lock1, lock2;
            _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), out lock1);
            _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), out lock2);

            Assert.AreEqual(lock1, lock2);
        }

        [Test]
        public void ReleasingSingleLockDeletesRecord() {
            IDistributedLock lock1;
            _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), out lock1);

            lock1.Dispose();
            var lockRecord = _distributedLockRepository.Table.FirstOrDefault();

            Assert.That(lockRecord, Is.Null);
        }

        [Test]
        public void ReleasingFirstLockDoesntDeleteRecord() {
            IDistributedLock lock1, lock2;
            _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), out lock1);
            _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), out lock2);

            lock1.Dispose();
            var lockRecord = _distributedLockRepository.Table.FirstOrDefault();
            Assert.That(lockRecord, Is.Not.Null);

            lock2.Dispose();
            lockRecord = _distributedLockRepository.Table.FirstOrDefault();
            Assert.That(lockRecord, Is.Null);
        }
        
        [Test]
        public void TryAcquiringLockTwiceFails() {
            IDistributedLock @lock;
            _applicationEnvironment.MachineName = "Orchard Test Machine 1";
            var attempt1 = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), out @lock);
            _applicationEnvironment.MachineName = "Orchard Test Machine 2";
            var attempt2 = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), out @lock);

            Assert.That(attempt1, Is.True);
            Assert.That(attempt2, Is.False);
        }

        [Test]
        public void TryAcquiringNonExpiredActiveLockFails() {
            IDistributedLock @lock;
            CreateNonExpiredActiveLock("Other Machine");
            var success = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromHours(1), out @lock);

            Assert.That(success, Is.False);
        }

        [Test]
        public void TryAcquiringNonExpiredButInactiveLockFromOtherMachineFails() {
            IDistributedLock @lock;
            CreateNonExpiredActiveLock("Other Machine");
            var success = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromHours(1), out @lock);

            Assert.That(success, Is.False);
        }

        [Test]
        public void TryAcquiringNonExpiredButInactiveLockFromSameMachineSucceeds() {
            IDistributedLock @lock;
            CreateNonExpiredActiveLock("Orchard Machine");
            var success = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromHours(1), out @lock);

            Assert.That(success, Is.True);
        }

        [Test]
        public void TryAcquiringExpiredButActiveLockSucceeds() {
            IDistributedLock @lock;
            CreateExpiredButActiveLock("Other Machine");
            var success = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromHours(1), out @lock);

            Assert.That(success, Is.True);
        }

        [Test]
        public void TryAcquiringNonExpiredAndActiveLockFromCurrentOwnerSucceeds() {
            IDistributedLock @lock;
            CreateNonExpiredActiveLock(GetMachineName());
            var success = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromHours(1), out @lock);

            Assert.That(success, Is.True);
        }

        [Test]
        public void AcquiringNonExpiredAndActiveLockFromDifferentOwnerThrowsTimeoutException() {
            CreateNonExpiredActiveLock("Other Machine");
            Assert.Throws<TimeoutException>(() => _distributedLockService.AcquireLock(LockName, TimeSpan.FromHours(1), TimeSpan.Zero));
        }

        [Test]
        public void MultipleAcquisitionsFromDifferentMachinesShouldFail() {
            IDistributedLock @lock;
            _applicationEnvironment.MachineName = "Orchard Test Machine 1";
            var attempt1 = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromMinutes(60), out @lock);
            _applicationEnvironment.MachineName = "Orchard Test Machine 2";
            var attempt2 = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromMinutes(60), out @lock);

            Assert.That(attempt1, Is.True);
            Assert.That(attempt2, Is.False);
        }

        [Test]
        public void MultipleAcquisitionsFromDifferentMachinesOnDifferentTenantShouldSucceed() {
            IDistributedLock @lock;
            _applicationEnvironment.MachineName = "Orchard Test Machine 1";
            var attempt1 = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), out @lock);
            _applicationEnvironment.MachineName = "Orchard Test Machine 2";
            _shellSettings.Name = "Foo";
            var attempt2 = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), out @lock);

            Assert.That(attempt1, Is.True);
            Assert.That(attempt2, Is.True);
        }

        [Test]
        public void MultithreadedAcquisitionsShouldNotCauseTransactionErrors() {
            var tasks = new List<Task>();

            for (var i = 0; i < 10; i++) {
                var task = Task.Factory.StartNew(() => {
                    IDistributedLock @lock;
                    Assert.DoesNotThrow(() => _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromHours(1), out @lock));
                });

                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
        }

        [Test]
        public void TryAcquireActiveLockWithNullTimeoutReturnsFalseImmediately() {
            CreateNonExpiredActiveLock("Other Machine");

            IDistributedLock @lock;
            var acquired = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromMinutes(1), out @lock);
            
            Assert.That(acquired, Is.False);
        }

        [Test]
        public void ActiveLockWithUndefinedValidUntilNeverExpires() {
            CreateNonExpiredActiveLockThatNeverExpires("Other Machine");

            _clock.Advance(DateTime.MaxValue - _clock.UtcNow); // Fast forward to the End of Time.
            IDistributedLock @lock;
            var acquired = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromMinutes(1), out @lock);

            Assert.That(acquired, Is.False);
        }

        [Test]
        public void ActiveLockWithUndefinedValidUntilNeverExpiresUntilReleased() {
            IDistributedLock @lock;

            // Create a never expiring lock.
            _applicationEnvironment.MachineName = "Orchard Test Machine 1";
            var attempt1 = _distributedLockService.TryAcquireLock(LockName, maxValidFor: null, timeout: null, dLock: out @lock);

            // Release the lock.
            @lock.Dispose();

            // Acquire the lock from another machine.
            _applicationEnvironment.MachineName = "Orchard Test Machine 2";
            var attempt2 = _distributedLockService.TryAcquireLock(LockName, maxValidFor: null, timeout: null, dLock: out @lock);

            // Validate the results.
            Assert.That(attempt1, Is.True);
            Assert.That(attempt2, Is.True);
        }

        private DistributedLockRecord CreateLockRecord(DateTime createdUtc, DateTime? validUntilUtc, string machineName) {
            var record = new DistributedLockRecord {
                Name = String.Format("DistributedLock:{0}:{1}", ShellSettings.DefaultName, LockName),
                CreatedUtc = createdUtc,
                ValidUntilUtc = validUntilUtc,
                MachineName = machineName,
            };

            _distributedLockRepository.Create(record);
            _transactionManager.RequireNew();
            return record;
        }

        private DistributedLockRecord CreateNonExpiredActiveLock(string machineName) {
            var now = _clock.UtcNow;
            return CreateLockRecord(now, now + TimeSpan.FromHours(1), machineName);
        }

        private DistributedLockRecord CreateExpiredButActiveLock(string machineName) {
            var now = _clock.UtcNow;
            return CreateLockRecord(now, now - TimeSpan.FromHours(1), machineName);
        }

        private DistributedLockRecord CreateNonExpiredActiveLockThatNeverExpires(string machineName) {
            var now = _clock.UtcNow;
            return CreateLockRecord(now, null, machineName);
        }

        private string GetMachineName() {
            return _applicationEnvironment.GetEnvironmentIdentifier();
        }
    }
}
