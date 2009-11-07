using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using NUnit.Framework;
using Orchard.Environment;

namespace Orchard.Tests.Environment {
    [TestFixture]
    public class OrchardStarterTests {
        [Test]
        public void DefaultOrchardHostInstanceReturnedByCreateHost() {
            var host = OrchardStarter.CreateHost(b => b.Register(new ControllerBuilder()));
            Assert.That(host, Is.TypeOf<DefaultOrchardHost>());
        }
    }
}
