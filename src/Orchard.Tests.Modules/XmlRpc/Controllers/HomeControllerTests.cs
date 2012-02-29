using System.Xml.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.Core.XmlRpc;
using Orchard.Core.XmlRpc.Controllers;
using Orchard.Core.XmlRpc.Models;
using Orchard.Core.XmlRpc.Services;

namespace Orchard.Tests.Modules.XmlRpc.Controllers {
    [TestFixture]
    public class HomeControllerTests {
        [Test]
        public void RequestShouldBeDispatchedToAllHandlers() {
            var thing1 = new StubHandler();
            var thing2 = new StubHandler();
            
            var builder = new ContainerBuilder();
            //builder.RegisterModule(new ImplicitCollectionSupportModule());
            builder.RegisterType<HomeController>();
            builder.RegisterType<XmlRpcReader>().As<IXmlRpcReader>();
            builder.RegisterType<XmlRpcWriter>().As<IXmlRpcWriter>();
            builder.RegisterInstance(thing1).As<IXmlRpcHandler>();
            builder.RegisterInstance(thing2).As<IXmlRpcHandler>();

            var container = builder.Build();

            var controller = container.Resolve<HomeController>();

            Assert.That(thing1.ProcessCalls, Is.EqualTo(0));
            Assert.That(thing2.ProcessCalls, Is.EqualTo(0));

            var result = controller.ServiceEndpoint(new XRpcMethodCall());
            Assert.That(result, Is.Not.Null);
            Assert.That(thing1.ProcessCalls, Is.EqualTo(1));
            Assert.That(thing2.ProcessCalls, Is.EqualTo(1));

        }

        public class StubHandler : IXmlRpcHandler {
            public void SetCapabilities(XElement element) {}

            public void Process(XmlRpcContext context) {
                ProcessCalls++;
                context.Response = new XRpcMethodResponse();
            }

            public int ProcessCalls { get; set; }
        }
    }
}