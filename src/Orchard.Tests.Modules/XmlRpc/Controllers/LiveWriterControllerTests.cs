using System.Web.Mvc;
using System.Xml.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.Core.XmlRpc;
using Orchard.Core.XmlRpc.Controllers;
using Orchard.Core.XmlRpc.Models;
using Orchard.Core.XmlRpc.Services;

namespace Orchard.Tests.Modules.XmlRpc.Controllers {
    [TestFixture]
    public class LiveWriterControllerTests {
        [Test]
        public void HandlersShouldSetCapabilitiesForManifest() {
            var thing = new StubHandler();
            var thingToo = new StubTooHandler();
            
            var builder = new ContainerBuilder();
            builder.RegisterType<LiveWriterController>();
            builder.RegisterType<XmlRpcReader>().As<IXmlRpcReader>();
            builder.RegisterType<XmlRpcWriter>().As<IXmlRpcWriter>();
            builder.RegisterInstance(thing).As<IXmlRpcHandler>();
            builder.RegisterInstance(thingToo).As<IXmlRpcHandler>();

            var container = builder.Build();

            var controller = container.Resolve<LiveWriterController>();
            var result = controller.Manifest() as ContentResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.StringContaining("<supportsGetTags>No</supportsGetTags>"));
            Assert.That(result.Content, Is.StringContaining("<keywordsAsTags>Yes</keywordsAsTags>"));
            Assert.That(result.Content, Is.StringContaining("<supportsKeywords>Maybe</supportsKeywords>"));

        }

        public class StubHandler : IXmlRpcHandler {
            public void SetCapabilities(XElement options) {
                const string manifestUri = "http://schemas.microsoft.com/wlw/manifest/weblog";
                options.SetElementValue(XName.Get("supportsGetTags", manifestUri), "No");
                options.SetElementValue(XName.Get("keywordsAsTags", manifestUri), "Yes");
            }

            public void Process(XmlRpcContext context) { }

            public int ProcessCalls { get; set; }
        }

        public class StubTooHandler : IXmlRpcHandler {
            public void SetCapabilities(XElement options) {
                const string manifestUri = "http://schemas.microsoft.com/wlw/manifest/weblog";
                options.SetElementValue(XName.Get("supportsKeywords", manifestUri), "Maybe");
            }

            public void Process(XmlRpcContext context) { }

            public int ProcessCalls { get; set; }
        }
    }
}