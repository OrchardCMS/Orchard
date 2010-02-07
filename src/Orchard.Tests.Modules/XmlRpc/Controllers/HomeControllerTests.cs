using System.Xml.Linq;
using Autofac.Builder;
using Autofac.Modules;
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
            builder.RegisterModule(new ImplicitCollectionSupportModule()); ;
            builder.Register<HomeController>();
            builder.Register<XmlRpcReader>().As<IMapper<XElement, XRpcMethodCall>>();
            builder.Register<XmlRpcWriter>().As<IMapper<XRpcMethodResponse, XElement>>();
            builder.Register(thing1).As<IXmlRpcHandler>();
            builder.Register(thing2).As<IXmlRpcHandler>();

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
            public void Process(XmlRpcContext context) {
                ProcessCalls++;
                context.Response = new XRpcMethodResponse();
            }

            public int ProcessCalls { get; set; }
        }
    }
}