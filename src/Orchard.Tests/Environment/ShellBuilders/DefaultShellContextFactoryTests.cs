using Autofac;
using Autofac.Core.Registration;
using Moq;
using NUnit.Framework;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;
using Orchard.Tests.Utility;

namespace Orchard.Tests.Environment.ShellBuilders {
    [TestFixture]
    public class DefaultShellContextFactoryTests {
        private IContainer _container;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<ShellContextFactory>().As<IShellContextFactory>();
            builder.RegisterAutoMocking(Moq.MockBehavior.Strict);
            _container = builder.Build();
        }

        [Test]
        public void NormalExecutionReturnsExpectedObjects() {
            var settings = new ShellSettings { Name = "Default" };
            var topologyDescriptor = new ShellTopologyDescriptor { SerialNumber = 6655321 };
            var topology = new ShellTopology();
            var shellLifetimeScope = _container.BeginLifetimeScope("shell");

            _container.Mock<ITopologyDescriptorCache>()
                .Setup(x => x.Fetch("Default"))
                .Returns(topologyDescriptor);

            _container.Mock<ICompositionStrategy>()
                .Setup(x => x.Compose(topologyDescriptor))
                .Returns(topology);

            _container.Mock<IShellContainerFactory>()
                .Setup(x => x.CreateContainer(settings, topology))
                .Returns(shellLifetimeScope );

            _container.Mock<ITopologyDescriptorManager>()
                .Setup(x => x.GetTopologyDescriptor())
                .Returns(topologyDescriptor);

            var factory = _container.Resolve<IShellContextFactory>();

            var context = factory.Create(settings);

            Assert.That(context.Settings, Is.SameAs(settings));
            Assert.That(context.TopologyDescriptor, Is.SameAs(topologyDescriptor));
            Assert.That(context.Topology, Is.SameAs(topology));
            Assert.That(context.LifetimeScope, Is.SameAs(shellLifetimeScope));
            Assert.That(context.Shell, Is.SameAs(shellLifetimeScope.Resolve<IOrchardShell>()));
        }

        [Test]
        public void NullSettingsReturnsSetupContext() {
            var topology = new ShellTopology();

            _container.Mock<ICompositionStrategy>()
                .Setup(x => x.Compose(It.IsAny<ShellTopologyDescriptor>()))
                .Returns(topology);

            _container.Mock<IShellContainerFactory>()
                .Setup(x => x.CreateContainer(It.IsAny<ShellSettings>(), topology))
                .Returns(_container.BeginLifetimeScope("shell"));

            var factory = _container.Resolve<IShellContextFactory>();
            var context = factory.Create(null);

            Assert.That(context.TopologyDescriptor.EnabledFeatures, Has.Some.With.Property("Name").EqualTo("Setup"));
        }
    }
}
