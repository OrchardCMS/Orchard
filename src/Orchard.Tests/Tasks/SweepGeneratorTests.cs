using System;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Tasks;

namespace Orchard.Tests.Tasks {
    [TestFixture]
    public class SweepGeneratorTests {

        [Test]
        public void DoWorkShouldSendHeartbeatToTaskManager() {
            var taskManager = new Mock<IBackgroundService>();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(taskManager.Object);
            var container = builder.Build();

            var heartbeatSource = new SweepGenerator(container);
            heartbeatSource.DoWork();
            taskManager.Verify(x => x.Sweep(), Times.Once());
        }

        [Test]
        public void ActivatedEventShouldStartTimer() {
            var taskManager = new Mock<IBackgroundService>();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(taskManager.Object);
            var container = builder.Build();

            var heartbeatSource = new SweepGenerator(container) {
                Interval = TimeSpan.FromMilliseconds(25)
            };

            taskManager.Verify(x => x.Sweep(), Times.Never());
            heartbeatSource.Activated();
            System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(80));
            heartbeatSource.Terminating();
            taskManager.Verify(x => x.Sweep(), Times.AtLeastOnce());
        }
    }
}
