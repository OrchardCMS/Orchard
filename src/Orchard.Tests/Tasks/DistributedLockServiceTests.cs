using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using Orchard.Environment;
using Orchard.Tasks.Locking;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Tasks {
    [TestFixture]
    public class DistributedLockServiceTests : ContainerTestBase {
        private DistributedLockService _distributedLockService;

        protected override void Register(ContainerBuilder builder) {
            builder.RegisterType<StubWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<StubMachineNameProvider>().As<IMachineNameProvider>();
            builder.RegisterType<StubDistributedLock>().As<IDistributedLock>();
            builder.RegisterType<DistributedLockService>().AsSelf();
            builder.RegisterInstance(new Work<IDistributedLock>(resolve => _container.Resolve<IDistributedLock>())).AsSelf();
        }

        protected override void Resolve(ILifetimeScope container) {
            _distributedLockService = container.Resolve<DistributedLockService>();
        }


        [Test]
        public void AcquiringLockSucceeds() {
            IDistributedLock @lock;
            var lockAcquired = _distributedLockService.TryAcquireLock("Test", TimeSpan.FromSeconds(60), TimeSpan.Zero, out @lock);
            
            Assert.That(lockAcquired, Is.True);
        }

        [Test]
        public void AcquiringLockTwiceFails() {
            IDistributedLock @lock;
            var attempt1 = _distributedLockService.TryAcquireLock("Test", TimeSpan.FromSeconds(60), TimeSpan.Zero, out @lock);
            var attempt2 = _distributedLockService.TryAcquireLock("Test", TimeSpan.FromSeconds(60), TimeSpan.Zero, out @lock);

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
                    var attempt = _distributedLockService.TryAcquireLock("Test", TimeSpan.FromSeconds(60), TimeSpan.Zero, out @lock);
                    attempts.Add(attempt);
                });

                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            Assert.That(attempts.Count(x => x == true), Is.EqualTo(1));
        }
    }
}
