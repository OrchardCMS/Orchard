using System;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Environment;
using Orchard.Mvc;
using Orchard.Tasks;
using Orchard.Tests.Stubs;
using Orchard.Tests.Utility;

namespace Orchard.Tests.Tasks {
    [TestFixture]
    public class SweepGeneratorTests : ContainerTestBase {
        protected override void Register(ContainerBuilder builder) {
            builder.RegisterAutoMocking(MockBehavior.Loose);
            builder.RegisterModule(new WorkContextModule());
            builder.RegisterType<WorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<SweepGenerator>();
        }

        protected override void Resolve(ILifetimeScope container) {
            container.Mock<IHttpContextAccessor>()
                .Setup(x => x.Current())
                .Returns(() => null);

            container.Mock<IWorkContextEvents>()
                .Setup(x => x.Started());
        }

        [Test]
        public void DoWorkShouldSendHeartbeatToTaskManager() {
            var heartbeatSource = _container.Resolve<SweepGenerator>();
            heartbeatSource.DoWork();
            _container.Resolve<Mock<IBackgroundService>>()
                .Verify(x => x.Sweep(), Times.Once());
        }

        [Test]
        public void ActivatedEventShouldStartTimer() {

            var heartbeatSource = _container.Resolve<SweepGenerator>();
            heartbeatSource.Interval = TimeSpan.FromMilliseconds(25);

            _container.Resolve<Mock<IBackgroundService>>()
                .Verify(x => x.Sweep(), Times.Never());

            heartbeatSource.Activate();
            System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(80));
            heartbeatSource.Terminate();

            _container.Resolve<Mock<IBackgroundService>>()
                .Verify(x => x.Sweep(), Times.AtLeastOnce());
        }
    }
}
