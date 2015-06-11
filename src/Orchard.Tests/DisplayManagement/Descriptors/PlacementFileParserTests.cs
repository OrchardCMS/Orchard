using System.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.DisplayManagement.Descriptors.ShapePlacementStrategy;
using Orchard.FileSystems.WebSite;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.DisplayManagement.Descriptors {
    [TestFixture]
    public class PlacementFileParserTests : ContainerTestBase {
        private IPlacementFileParser _parser;
        private InMemoryWebSiteFolder _folder;

        protected override void Register(Autofac.ContainerBuilder builder) {
            builder.RegisterType<PlacementFileParser>().As<IPlacementFileParser>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();
            builder.RegisterType<InMemoryWebSiteFolder>().As<IWebSiteFolder>()
                .As<InMemoryWebSiteFolder>().InstancePerLifetimeScope();
        }


        protected override void Resolve(ILifetimeScope container) {
            _parser = container.Resolve<IPlacementFileParser>();
            _folder = container.Resolve<InMemoryWebSiteFolder>();
        }

        [Test]
        public void ParsingMissingFileIsNull() {
            var result = _parser.Parse("~/hello.xml");
            Assert.That(result, Is.Null);
        }

        [Test]
        public void ParsingEmptyFileAsNothing() {
            _folder.Contents["~/hello.xml"] = "<Placement/>";
            var result = _parser.Parse("~/hello.xml");
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Nodes, Is.Not.Null);
            Assert.That(result.Nodes.Count(), Is.EqualTo(0));
        }

        [Test]
        public void ItemsComeBackAsPlacementNodes() {
            _folder.Contents["~/hello.xml"] = @"
<Placement>
  <Match ContentType=""BlogPost""/>
  <Match ContentType=""Page""/>
</Placement>
";
            var result = _parser.Parse("~/hello.xml");
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Nodes, Is.Not.Null);
            Assert.That(result.Nodes.Count(), Is.EqualTo(2));
        }


        [Test]
        public void NestedItemsComeBackAsNestedNodes() {
            _folder.Contents["~/hello.xml"] = @"
<Placement>
  <Match ContentType=""BlogPost"">
    <Match DisplayType=""Detail""/>
  </Match>
  <Match ContentType=""Page""/>
</Placement>
";
            var result = _parser.Parse("~/hello.xml");
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Nodes, Is.Not.Null);
            Assert.That(result.Nodes.Count(), Is.EqualTo(2));
            Assert.That(result.Nodes.First().Nodes.Count(), Is.EqualTo(1));
            Assert.That(result.Nodes.Last().Nodes.Count(), Is.EqualTo(0));
        }

        [Test]
        public void EachPlaceAttributeIsShapeLocation() {
            _folder.Contents["~/hello.xml"] = @"
<Place Foo=""Header"" Bar=""Content:after""/>
";
            var result = _parser.Parse("~/hello.xml");
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Nodes, Is.Not.Null);
            Assert.That(result.Nodes.Count(), Is.EqualTo(2));
            var foo = result.Nodes.OfType<PlacementShapeLocation>().Single(x=>x.ShapeType == "Foo");
            var bar = result.Nodes.OfType<PlacementShapeLocation>().Single(x=>x.ShapeType == "Bar");
            Assert.That(foo.Location, Is.EqualTo("Header"));
            Assert.That(bar.Location, Is.EqualTo("Content:after"));
        }
    }
}
