using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Orchard.Mvc;
using Orchard.Extensions;
using Autofac.Core;

namespace Orchard.Tests.Mvc {
   [TestFixture] public class OrchardControllerIdentificationStrategyTests {
       [Test]
       public void IdentificationStrategyAddsAssemblyNameAsAreaPrefixToControllerNames() {
           var strategy = new OrchardControllerIdentificationStrategy(Enumerable.Empty<ExtensionEntry>());
           var service = strategy.ServiceForControllerType(typeof (StrategyTestingController));
           Assert.That(service, Is.TypeOf<NamedService>());
           Assert.That(((NamedService)service).ServiceName, Is.EqualTo("controller.orchard.framework.tests.strategytesting"));
       }
    }

    public class StrategyTestingController:Controller {
    }
}
