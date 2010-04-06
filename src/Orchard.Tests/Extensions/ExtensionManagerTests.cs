using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.Extensions;
using Yaml.Grammar;

namespace Orchard.Tests.Extensions {
    [TestFixture]
    public class ExtensionManagerTests {
        private IContainer _container;
        private IExtensionManager _manager;
        private StubFolders _folders;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            _folders = new StubFolders();
            //builder.RegisterModule(new ImplicitCollectionSupportModule());
            builder.RegisterInstance(_folders).As<IExtensionFolders>();
            builder.RegisterType<ExtensionManager>().As<IExtensionManager>();
            _container = builder.Build();
            _manager = _container.Resolve<IExtensionManager>();
        }

        public class StubFolders : IExtensionFolders {
            public StubFolders() {
                Manifests = new Dictionary<string, string>();
            }

            public IDictionary<string, string> Manifests { get; set; }

            public IEnumerable<string> ListNames() {
                return Manifests.Keys;
            }

            public ParseResult ParseManifest(string name) {
                var parser = new YamlParser();
                bool success;
                var stream = parser.ParseYamlStream(new TextInput(Manifests[name]), out success);
                if (success) {
                    return new ParseResult {
                        Location = "~/InMemory",
                        Name = name,
                        YamlDocument = stream.Documents.Single()
                    };
                }
                return null;
            }
        }


        [Test]
        public void AvailableExtensionsShouldFollowCatalogLocations() {
            _folders.Manifests.Add("foo", "name: Foo");
            _folders.Manifests.Add("bar", "name: Bar");
            _folders.Manifests.Add("frap", "name: Frap");
            _folders.Manifests.Add("quad", "name: Quad");

            var available = _manager.AvailableExtensions();

            Assert.That(available.Count(), Is.EqualTo(4));
            Assert.That(available, Has.Some.Property("Name").EqualTo("foo"));
        }

        [Test]
        public void ExtensionDescriptorsShouldHaveNameAndVersion() {

            _folders.Manifests.Add("Sample", @"
name: Sample Extension
version: 2.x
");

            var descriptor = _manager.AvailableExtensions().Single();
            Assert.That(descriptor.Name, Is.EqualTo("Sample"));
            Assert.That(descriptor.DisplayName, Is.EqualTo("Sample Extension"));
            Assert.That(descriptor.Version, Is.EqualTo("2.x"));
        }

        [Test]
        public void ExtensionDescriptorsShouldBeParsedForMinimalModuleTxt() {

            _folders.Manifests.Add("SuperWiki", @"
name: SuperWiki
version: 1.0.3
orchardversion: 1
features:
  SuperWiki: 
    Description: My super wiki module for Orchard.
");

            var descriptor = _manager.AvailableExtensions().Single();
            Assert.That(descriptor.Name, Is.EqualTo("SuperWiki"));
            Assert.That(descriptor.Version, Is.EqualTo("1.0.3"));
            Assert.That(descriptor.OrchardVersion, Is.EqualTo("1"));
            Assert.That(descriptor.Features.Count(), Is.EqualTo(1));
            Assert.That(descriptor.Features.First().Name, Is.EqualTo("SuperWiki"));
            Assert.That(descriptor.Features.First().ExtensionName, Is.EqualTo("SuperWiki"));
            Assert.That(descriptor.Features.First().Description, Is.EqualTo("My super wiki module for Orchard."));
        }

    }
}
