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
            var topologyDescriptor = new ShellDescriptor { SerialNumber = 6655321 };
            var topology = new ShellTopology();
            var shellLifetimeScope = _container.BeginLifetimeScope("shell");

            _container.Mock<IShellDescriptorCache>()
                .Setup(x => x.Fetch("Default"))
                .Returns(topologyDescriptor);

            _container.Mock<ICompositionStrategy>()
                .Setup(x => x.Compose(settings, topologyDescriptor))
                .Returns(topology);

            _container.Mock<IShellContainerFactory>()
                .Setup(x => x.CreateContainer(settings, topology))
                .Returns(shellLifetimeScope);

            _container.Mock<IShellDescriptorManager>()
                .Setup(x => x.GetShellDescriptor())
                .Returns(topologyDescriptor);

            var factory = _container.Resolve<IShellContextFactory>();

            var context = factory.CreateShellContext(settings);

            Assert.That(context.Settings, Is.SameAs(settings));
            Assert.That(context.Descriptor, Is.SameAs(topologyDescriptor));
            Assert.That(context.Topology, Is.SameAs(topology));
            Assert.That(context.LifetimeScope, Is.SameAs(shellLifetimeScope));
            Assert.That(context.Shell, Is.SameAs(shellLifetimeScope.Resolve<IOrchardShell>()));
        }

        [Test]
        public void CreatingSetupContextUsesOrchardSetupFeature() {
            var settings = default(ShellSettings);
            var descriptor = default(ShellDescriptor);
            var topology = new ShellTopology();

            _container.Mock<ICompositionStrategy>()
                .Setup(x => x.Compose(It.IsAny<ShellSettings>(), It.IsAny<ShellDescriptor>()))
                .Callback((ShellSettings s, ShellDescriptor d) => {
                    settings = s;
                    descriptor = d;
                })
                .Returns(topology);

            _container.Mock<IShellContainerFactory>()
                .Setup(x => x.CreateContainer(It.IsAny<ShellSettings>(), topology))
                .Returns(_container.BeginLifetimeScope("shell"));

            var factory = _container.Resolve<IShellContextFactory>();
            var context = factory.CreateSetupContext();

            Assert.That(context.Descriptor.EnabledFeatures, Has.Some.With.Property("Name").EqualTo("Orchard.Setup"));
        }
    }
}
