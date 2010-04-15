using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using Moq;
using NUnit.Framework;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;

namespace Orchard.Tests.Environment.ShellBuilders {
    [TestFixture]
    public class DefaultShellContextFactoryTests {
        private IContainer _container;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<DefaultShellContextFactory>().As<IShellContextFactory>();
            builder.RegisterSource(new MockServiceSource());
            _container = builder.Build();
        }

        [Test]
        public void NormalExecutionReturnsExpectedObjects() {
            var settings = new ShellSettings {Name = "Default"};
            var topologyDescriptor = new ShellTopologyDescriptor {SerialNumber = 6655321};
            var topology = new ShellTopology();
            ILifetimeScope shellLifetimeScope;

            _container.Mock<ITopologyDescriptorCache>()
                .Setup(x => x.Fetch("Default"))
                .Returns(topologyDescriptor);

            _container.Mock<ICompositionStrategy>()
                .Setup(x => x.Compose(topologyDescriptor))
                .Returns(topology);

            _container.Mock<IShellContainerFactory>()
                .Setup(x => x.CreateContainer(topology))
                .Returns(shellLifetimeScope = _container.BeginLifetimeScope("shell"));

            _container.Mock<ITopologyDescriptorProvider>()
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
    }

    class MockServiceSource : IRegistrationSource {
        public IEnumerable<IComponentRegistration> RegistrationsFor(
            Service service,
            Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor) {

            var swt = service as IServiceWithType;
            if (swt == null)
                yield break;
            var st = swt.ServiceType;

            if (st.IsGenericType && st.GetGenericTypeDefinition() == typeof(Mock<>)) {
                yield return RegistrationBuilder.ForType(st).SingleInstance().CreateRegistration();
            }
            else if (st.IsInterface) {
                yield return RegistrationBuilder.ForDelegate(
                    (ctx, p) => {
                        Trace.WriteLine(string.Format("Mocking {0}", st));
                        var mt = typeof(Mock<>).MakeGenericType(st);
                        var m = (Mock)ctx.Resolve(mt);
                        return m.Object;
                    })
                    .As(service)
                    .SingleInstance()
                    .CreateRegistration();

            }
        }
    }

    public static class MockExtensions {
        public static Mock<T> Mock<T>(this IContainer container) where T : class {
            return container.Resolve<Mock<T>>();
        }
    }
}
