using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using NHibernate.Linq;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Services;
using Orchard.Tasks.Locking.Records;
using Orchard.Tasks.Locking.Services;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Tasks {
    [TestFixture]
    public class DistributedLockServiceTests : DatabaseEnabledTestsBase {
        private const string LockName = "Orchard Test Lock";
        private DistributedLockService _distributedLockService;
        private StubMachineNameProvider _machineNameProvider;
        private StubThreadProvider _threadProvider;
        private IRepository<DistributedLockRecord> _distributedLockRepository;
        private ITransactionManager _transactionManager;
        

        protected override IEnumerable<Type> DatabaseTypes {
            get { yield return typeof(DistributedLockRecord); }
        }

        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<StubClock>().As<IClock>();
            builder.RegisterType<StubMachineNameProvider>().As<IMachineNameProvider>().SingleInstance();
            builder.RegisterType<StubThreadProvider>().As<IThreadProvider>().SingleInstance();
            builder.RegisterType<DistributedLockService>().AsSelf();
        }

        public override void Init() {
            base.Init();
            _distributedLockService = _container.Resolve<DistributedLockService>();
            _machineNameProvider = (StubMachineNameProvider)_container.Resolve<IMachineNameProvider>();
            _threadProvider = (StubThreadProvider)_container.Resolve<IThreadProvider>();
            _distributedLockRepository = _container.Resolve<IRepository<DistributedLockRecord>>();
            _transactionManager = _container.Resolve<ITransactionManager>();
        }

        [Test]
        public void TryAcquiringLockSucceeds() {
            DistributedLock @lock;
            var lockAcquired = _distributedLockService.TryAcquireLockForMachine(LockName, TimeSpan.FromSeconds(60), null, out @lock);

            Assert.That(lockAcquired, Is.True);
        }

        [Test]
        public void TryAcquiringLockTwiceOnSameMachineSucceeds() {
            DistributedLock @lock;
            var attempt1 = _distributedLockService.TryAcquireLockForMachine(LockName, TimeSpan.FromSeconds(60), null, out @lock);
            var attempt2 = _distributedLockService.TryAcquireLockForMachine(LockName, TimeSpan.FromSeconds(60), null, out @lock);

            Assert.That(attempt1, Is.True);
            Assert.That(attempt2, Is.True);
        }

        [Test]
        public void TryAcquiringLockTwiceOnSameMachineIncreasesLockCountTwice() {
            DistributedLock @lock;
            _distributedLockService.TryAcquireLockForMachine(LockName, TimeSpan.FromSeconds(60), null, out @lock);
            _distributedLockService.TryAcquireLockForMachine(LockName, TimeSpan.FromSeconds(60), null, out @lock);

            var lockId = Int32.Parse(@lock.Id);
            var lockRecord = _distributedLockRepository.Get(lockId);
            Assert.That(lockRecord.Count, Is.EqualTo(2));
        }

        [Test]
        public void ReleasingLockDecreasesLockCount() {
            DistributedLock @lock;
            _distributedLockService.TryAcquireLockForMachine(LockName, TimeSpan.FromSeconds(60), null, out @lock);
            _distributedLockService.TryAcquireLockForMachine(LockName, TimeSpan.FromSeconds(60), null, out @lock);

            var lockId = Int32.Parse(@lock.Id);
            var lockRecord = _distributedLockRepository.Get(lockId);
            
            _distributedLockService.ReleaseLock(@lock);
            _session.Refresh(lockRecord);
            Assert.That(lockRecord.Count, Is.EqualTo(1));

            _distributedLockService.ReleaseLock(@lock);
            _session.Refresh(lockRecord);
            Assert.That(lockRecord.Count, Is.EqualTo(0));
        }

        [Test]
        public void TryAcquiringLockTwiceFails() {
            DistributedLock @lock;
            _machineNameProvider.MachineName = "Orchard Test Machine 1";
            var attempt1 = _distributedLockService.TryAcquireLockForMachine(LockName, TimeSpan.FromSeconds(60), null, out @lock);
            _machineNameProvider.MachineName = "Orchard Test Machine 2";
            var attempt2 = _distributedLockService.TryAcquireLockForMachine(LockName, TimeSpan.FromSeconds(60), null, out @lock);

            Assert.That(attempt1, Is.True);
            Assert.That(attempt2, Is.False);
        }

        [Test]
        public void TryAcquiringNonExpiredActiveLockFails() {
            DistributedLock @lock;
            CreateNonExpiredActiveLock("Other Machine", threadId: null);
            var success = _distributedLockService.TryAcquireLockForMachine(LockName, TimeSpan.FromHours(1), null, out @lock);

            Assert.That(success, Is.False);
        }

        [Test]
        public void TryAcquiringNonExpiredButInactiveLockSucceeds() {
            DistributedLock @lock;
            CreateNonExpiredButInactiveLock("Other Machine", threadId: null);
            var success = _distributedLockService.TryAcquireLockForMachine(LockName, TimeSpan.FromHours(1), null, out @lock);

            Assert.That(success, Is.True);
        }

        [Test]
        public void TryAcquiringExpiredButActiveLockSucceeds() {
            DistributedLock @lock;
            CreateExpiredButActiveLock("Other Machine", threadId: null);
            var success = _distributedLockService.TryAcquireLockForMachine(LockName, TimeSpan.FromHours(1), null, out @lock);

            Assert.That(success, Is.True);
        }

        [Test]
        public void TryAcquiringNonExpiredAndActiveLockFromCurrentOwnerSucceeds() {
            DistributedLock @lock;
            CreateNonExpiredActiveLock(GetMachineName(), threadId: null);
            var success = _distributedLockService.TryAcquireLockForMachine(LockName, TimeSpan.FromHours(1), null, out @lock);

            Assert.That(success, Is.True);
        }

        [Test]
        public void AcquiringNonExpiredAndActiveLockFromDifferentOwnerThrowsTimeoutException() {
            CreateNonExpiredActiveLock("Other Machine", threadId: null);
            Assert.Throws<TimeoutException>(() => _distributedLockService.AcquireLockForMachine(LockName, TimeSpan.FromHours(1), TimeSpan.Zero));
        }

        [Test]
        public void MultipleAcquisitionsFromDifferentMachinesShouldFail() {
            DistributedLock @lock;
            _machineNameProvider.MachineName = "Orchard Test Machine 1";
            var attempt1 = _distributedLockService.TryAcquireLockForMachine(LockName, TimeSpan.FromSeconds(60), null, out @lock);
            _machineNameProvider.MachineName = "Orchard Test Machine 2";
            var attempt2 = _distributedLockService.TryAcquireLockForMachine(LockName, TimeSpan.FromSeconds(60), null, out @lock);

            Assert.That(attempt1, Is.True);
            Assert.That(attempt2, Is.False);
        }

        [Test]
        public void MultithreadedAcquisitionsShouldNotCauseTransactionErrors() {
            var tasks = new List<Task>();

            for (var i = 0; i < 10; i++) {
                var task = Task.Factory.StartNew(() => {
                    DistributedLock @lock;
                    Assert.DoesNotThrow(() => _distributedLockService.TryAcquireLockForMachine(LockName, TimeSpan.FromHours(1), null, out @lock));
                });

                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
        }

        [Test]
        public void MixedScopeAcquisitionsShouldThrow() {
            DistributedLock @lock;
            Assert.DoesNotThrow(() => _distributedLockService.TryAcquireLockForMachine(LockName, TimeSpan.FromSeconds(60), null, out @lock));
            Assert.Throws<InvalidOperationException>(() => _distributedLockService.TryAcquireLockForThread(LockName, TimeSpan.FromSeconds(60), null, out @lock));
        }

        [Test]
        public void TryAcquireActiveLockWithNullTimeoutReturnsFalseImmediately() {
            CreateNonExpiredActiveLock("Other Machine", null);

            DistributedLock @lock;
            var acquired = _distributedLockService.TryAcquireLockForThread(LockName, TimeSpan.FromMinutes(1), null, out @lock);
            
            Assert.That(acquired, Is.False);
        }

        private DistributedLockRecord CreateLockRecord(int count, DateTime createdUtc, DateTime validUntilUtc, string machineName, int? threadId) {
            var record = new DistributedLockRecord {
                Name = LockName,
                Count = count,
                CreatedUtc = createdUtc,
                ValidUntilUtc = validUntilUtc,
                MachineName = machineName,
                ThreadId = threadId
            };

            _distributedLockRepository.Create(record);
            _transactionManager.RequireNew();
            return record;
        }

        private DistributedLockRecord CreateNonExpiredActiveLock(string machineName, int? threadId) {
            var now = _clock.UtcNow;
            return CreateLockRecord(1, now, now + TimeSpan.FromHours(1), machineName, threadId);
        }

        private DistributedLockRecord CreateNonExpiredButInactiveLock(string machineName, int? threadId) {
            var now = _clock.UtcNow;
            return CreateLockRecord(0, now, now + TimeSpan.FromHours(1), machineName, threadId);
        }

        private DistributedLockRecord CreateExpiredButActiveLock(string machineName, int? threadId) {
            var now = _clock.UtcNow;
            return CreateLockRecord(1, now, now - TimeSpan.FromHours(1), machineName, threadId);
        }

        private string GetMachineName() {
            return _machineNameProvider.GetMachineName();
        }

        private int GetThreadId() {
            return _threadProvider.GetCurrentThreadId();
        }
    }
}
