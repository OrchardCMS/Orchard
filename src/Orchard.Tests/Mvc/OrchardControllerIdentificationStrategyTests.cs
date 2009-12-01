using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Autofac;
using NUnit.Framework;
using Orchard.Mvc;
using Orchard.Packages;

namespace Orchard.Tests.Mvc {
   [TestFixture] public class OrchardControllerIdentificationStrategyTests {
       [Test]
       public void IdentificationStrategyAddsAssemblyNameAsAreaPrefixToControllerNames() {
           var strategy = new OrchardControllerIdentificationStrategy(Enumerable.Empty<PackageEntry>());
           var service = strategy.ServiceForControllerType(typeof (StrategyTestingController));
           Assert.That(service, Is.TypeOf<NamedService>());
           Assert.That(((NamedService)service).ServiceName, Is.EqualTo("controller.orchard.tests.strategytesting"));
       }
    }

    public class StrategyTestingController:Controller {
    }
}
