using System.Web;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.Mvc;
using Orchard.Tests.Stubs;
using Orchard.Tests.Utility;

namespace Orchard.Tests.Environment.ShellBuilders {
    [TestFixture]
    public class DefaultShellContextFactoryTests {
        private IContainer _container;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<ShellContextFactory>().As<IShellContextFactory>();
            builder.RegisterModule(new WorkContextModule());
            builder.RegisterType<WorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterAutoMocking(Moq.MockBehavior.Strict);
            _container = builder.Build();
        }

        [Test]
        public void NormalExecutionReturnsExpectedObjects() {
            var settings = new ShellSettings { Name = ShellSettings.DefaultName };
            var descriptor = new ShellDescriptor { SerialNumber = 6655321 };
            var blueprint = new ShellBlueprint();
            var shellLifetimeScope = _container.BeginLifetimeScope("shell");
            var httpContext = new StubHttpContext();

            _container.Mock<IShellDescriptorCache>()
                .Setup(x => x.Fetch(ShellSettings.DefaultName))
                .Returns(descriptor);

            _container.Mock<ICompositionStrategy>()
                .Setup(x => x.Compose(settings, descriptor))
                .Returns(blueprint);

            _container.Mock<IShellContainerFactory>()
                .Setup(x => x.CreateContainer(settings, blueprint))
                .Returns(shellLifetimeScope);

            _container.Mock<IShellDescriptorManager>()
                .Setup(x => x.GetShellDescriptor())
                .Returns(descriptor);

            _container.Mock<IWorkContextEvents>()
                .Setup(x => x.Started());

            _container.Mock<IHttpContextAccessor>()
                .Setup(x => x.Current())
                .Returns(default(HttpContextBase));

            var factory = _container.Resolve<IShellContextFactory>();

            var context = factory.CreateShellContext(settings);

            Assert.That(context.Settings, Is.SameAs(settings));
            Assert.That(context.Descriptor, Is.SameAs(descriptor));
            Assert.That(context.Blueprint, Is.SameAs(blueprint));
            Assert.That(context.LifetimeScope, Is.SameAs(shellLifetimeScope));
            Assert.That(context.Shell, Is.SameAs(shellLifetimeScope.Resolve<IOrchardShell>()));
        }

        [Test]
        public void CreatingSetupContextUsesOrchardSetupFeature() {
            var settings = default(ShellSettings);
            var descriptor = default(ShellDescriptor);
            var blueprint = new ShellBlueprint();

            _container.Mock<ICompositionStrategy>()
                .Setup(x => x.Compose(It.IsAny<ShellSettings>(), It.IsAny<ShellDescriptor>()))
                .Callback((ShellSettings s, ShellDescriptor d) => {
                    settings = s;
                    descriptor = d;
                })
                .Returns(blueprint);

            _container.Mock<IShellContainerFactory>()
                .Setup(x => x.CreateContainer(It.IsAny<ShellSettings>(), blueprint))
                .Returns(_container.BeginLifetimeScope("shell"));

            var factory = _container.Resolve<IShellContextFactory>();
            var context = factory.CreateSetupContext(new ShellSettings { Name = ShellSettings.DefaultName });

            Assert.That(context.Descriptor.Features, Has.Some.With.Property("Name").EqualTo("Orchard.Setup"));
        }
    }
}