using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
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
        private StubMachineNameProvider _stubMachineNameProvider;
        private IRepository<LockRecord> _lockRepository;

        protected override IEnumerable<Type> DatabaseTypes
        {
            get { yield return typeof (LockRecord); }
        }

        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<StubClock>().As<IClock>();
            //builder.RegisterType<StubWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<StubMachineNameProvider>().As<IMachineNameProvider>().SingleInstance();
            builder.RegisterType<StubDistributedLock>().As<IDistributedLock>();
            builder.RegisterType<DistributedLockService>().AsSelf();
            builder.RegisterInstance(new Work<IDistributedLock>(resolve => _container.Resolve<IDistributedLock>())).AsSelf();
        }

        public override void Init() {
            base.Init();
            _distributedLockService = _container.Resolve<DistributedLockService>();
            _stubMachineNameProvider = (StubMachineNameProvider)_container.Resolve<IMachineNameProvider>();
            _lockRepository = _container.Resolve<IRepository<LockRecord>>();
        }


        [Test]
        public void AcquiringLockSucceeds() {
            IDistributedLock @lock;
            var lockAcquired = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), TimeSpan.Zero, out @lock);
            
            Assert.That(lockAcquired, Is.True);
        }

        [Test]
        public void AcquiringLockTwiceOnSameMachineSucceeds() {
            IDistributedLock @lock;
            var attempt1 = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), TimeSpan.Zero, out @lock);
            var attempt2 = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), TimeSpan.Zero, out @lock);

            Assert.That(attempt1, Is.True);
            Assert.That(attempt2, Is.True);
        }

        [Test]
        public void AcquiringLockTwiceOnSameMachineIncreasesRefCountTwice() {
            IDistributedLock @lock;
            _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), TimeSpan.Zero, out @lock);
            _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), TimeSpan.Zero, out @lock);

            var lockRecord = _lockRepository.Get(@lock.Id);
            Assert.That(lockRecord.ReferenceCount, Is.EqualTo(2));
        }

        [Test]
        public void DisposingLockWillDecreaseRefCount() {
            IDistributedLock @lock;
            _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), TimeSpan.Zero, out @lock);
            _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), TimeSpan.Zero, out @lock);
            var lockRecord = _lockRepository.Get(@lock.Id);

            _distributedLockService.DisposeLock(@lock);
            Assert.That(lockRecord.ReferenceCount, Is.EqualTo(1));

            _distributedLockService.DisposeLock(@lock);
            Assert.That(lockRecord.ReferenceCount, Is.EqualTo(0));
        }

        [Test]
        public void AcquiringLockTwiceFails() {
            IDistributedLock @lock;
            _stubMachineNameProvider.MachineName = "Orchard Test Machine 1";
            var attempt1 = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), TimeSpan.Zero, out @lock);
            _stubMachineNameProvider.MachineName = "Orchard Test Machine 2";
            var attempt2 = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), TimeSpan.Zero, out @lock);

            Assert.That(attempt1, Is.True);
            Assert.That(attempt2, Is.False);
        }

        [Test]
        public void MultipleSimultaneousAcquisitionsShouldAllowOneLock() {
            var attempts = new List<bool>();
            var tasks = new List<Task>();

            foreach (var index in Enumerable.Range(0, 20)) {
                var task = Task.Factory.StartNew(() => {
                    IDistributedLock @lock;
                    _stubMachineNameProvider.MachineName = "Orchard Test Machine " + (index + 1);
                    var attempt = _distributedLockService.TryAcquireLock(LockName, TimeSpan.FromSeconds(60), TimeSpan.Zero, out @lock);
                    attempts.Add(attempt);
                });

                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            Assert.That(attempts.Count(x => x == true), Is.EqualTo(1));
        }
    }
}
