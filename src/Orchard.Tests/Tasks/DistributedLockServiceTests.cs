using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using Orchard.Environment;
using Orchard.Tasks.Locking.Services;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Tasks {
    [TestFixture]
    public class DistributedLockServiceTests : ContainerTestBase {
        private const string LockName = "Orchard Test Lock";
        private DistributedLockService _distributedLockService;
        private StubMachineNameProvider _stubMachineNameProvider;

        protected override void Register(ContainerBuilder builder) {
            builder.RegisterType<StubWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<StubMachineNameProvider>().As<IMachineNameProvider>().SingleInstance();
            builder.RegisterType<StubDistributedLock>().As<IDistributedLock>();
            builder.RegisterType<DistributedLockService>().AsSelf();
            builder.RegisterInstance(new Work<IDistributedLock>(resolve => _container.Resolve<IDistributedLock>())).AsSelf();
        }

        protected override void Resolve(ILifetimeScope container) {
            _distributedLockService = container.Resolve<DistributedLockService>();
            _stubMachineNameProvider = (StubMachineNameProvider)container.Resolve<IMachineNameProvider>();
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

            foreach (var index in Enumerable.Range(0, 10).AsParallel()) {
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
