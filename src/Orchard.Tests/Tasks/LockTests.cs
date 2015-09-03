using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Tasks.Locking.Services;

namespace Orchard.Tests.Tasks {
    [TestFixture]
    public class LockTests : ContainerTestBase {
        private const string LockName = "Orchard Test Lock";
        private Mock<DistributedLockService> _distributedLockServiceMock;
        private DistributedLock _lock;

        protected override void Register(ContainerBuilder builder) {
            _distributedLockServiceMock = new Mock<DistributedLockService>();
            builder.RegisterInstance(_distributedLockServiceMock.Object);
        }

        protected override void Resolve(ILifetimeScope container) {
            _lock = new DistributedLock(_distributedLockServiceMock.Object, LockName);
        }

        [Test]
        public void DisposeInvokesDistributedLockServiceDisposeLock() {
            _lock.Dispose();

            _distributedLockServiceMock.Verify(service => service.ReleaseDistributedLock(_lock), Times.Exactly(1));
        }

        [Test]
        public void DisposingMultipleTimesInvokesDistributedLockServiceDisposeLockOnce() {
            _lock.Dispose();
            _lock.Dispose();
            _lock.Dispose();

            _distributedLockServiceMock.Verify(service => service.ReleaseDistributedLock(_lock), Times.Exactly(1));
        }
    }
}