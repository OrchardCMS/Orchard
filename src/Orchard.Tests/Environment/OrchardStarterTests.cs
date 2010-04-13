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
    }
}
