using System;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Tasks.Locking.Services;

namespace Orchard.Tests.Tasks {
    [TestFixture]
    public class LockTests : ContainerTestBase {
        private const string LockName = "Orchard Test Lock";
        private const int LockId = 1;
        private Mock<IDistributedLockService> _distributedLockServiceMock;
        private Lock _lock;

        protected override void Register(ContainerBuilder builder) {
            _distributedLockServiceMock = new Mock<IDistributedLockService>();
            builder.RegisterInstance(_distributedLockServiceMock.Object);
        }

        protected override void Resolve(ILifetimeScope container) {
            _lock = new Lock(_distributedLockServiceMock.Object, LockName, LockId);
        }

        [Test]
        public void DisposeInvokesDistributedLockServiceDisposeLock() {
            _lock.Dispose();

            _distributedLockServiceMock.Verify(service => service.DisposeLock(_lock), Times.Exactly(1));
        }

        [Test]
        public void DisposingMultipleTimesInvokesDistributedLockServiceDisposeLockOnce() {
            _lock.Dispose();
            _lock.Dispose();
            _lock.Dispose();

            _distributedLockServiceMock.Verify(service => service.DisposeLock(_lock), Times.Exactly(1));
        }
    }
}