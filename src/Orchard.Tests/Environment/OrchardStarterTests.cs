using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using NUnit.Framework;
using Orchard.Environment;

namespace Orchard.Tests.Environment {
    [TestFixture]
    public class OrchardStarterTests {
        [Test]
        public void DefaultOrchardHostInstanceReturnedByCreateHost() {
            var host = OrchardStarter.CreateHost(b => b.RegisterInstance(new ControllerBuilder()));
            Assert.That(host, Is.TypeOf<DefaultOrchardHost>());
        }

        [Test]
        public void ContainerResolvesServicesInSameOrderTheyAreRegistered() {
            var container = OrchardStarter.CreateHostContainer(builder => {
                builder.RegisterType<Component1>().As<IServiceA>();
                builder.RegisterType<Component2>().As<IServiceA>();
            });
            var services = container.Resolve<IEnumerable<IServiceA>>();
            Assert.That(services.Count(), Is.EqualTo(2));
            Assert.That(services.First(), Is.TypeOf<Component1>());
            Assert.That(services.Last(), Is.TypeOf<Component2>());
        }

        [Test]
        public void MostRecentlyRegisteredServiceReturnsFromSingularResolve() {
            var container = OrchardStarter.CreateHostContainer(builder => {
                builder.RegisterType<Component1>().As<IServiceA>();
                builder.RegisterType<Component2>().As<IServiceA>();
            });
            var service = container.Resolve<IServiceA>();
            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.TypeOf<Component2>());
        }

        public interface IServiceA {}

        public class Component1 : IServiceA {}

        public class Component2 : IServiceA {}
    }
}