using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Autofac.Builder;
using NUnit.Framework;
using Orchard.Packages;
using Yaml.Grammar;

namespace Orchard.Tests.Packages {
    [TestFixture]
    public class PackageManagerTests {
        private IContainer _container;
        private IPackageManager _manager;
        private StubFolders _folders;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            _folders = new StubFolders();
            builder.Register(_folders).As<IPackageFolders>();
            builder.Register<PackageManager>().As<IPackageManager>();
            _container = builder.Build();
            _manager = _container.Resolve<IPackageManager>();
        }

        public class StubFolders : IPackageFolders {
            public StubFolders() {
                Manifests = new Dictionary<string, string>();
            }

            public IDictionary<string, string> Manifests { get; set; }

            public IEnumerable<string> ListNames() {
                return Manifests.Keys;
            }

            public YamlDocument ParseManifest(string name) {
                var parser = new YamlParser();
                bool success;
                var stream = parser.ParseYamlStream(new TextInput(Manifests[name]), out success);
                return success ? stream.Documents.Single() : null;
            }

        }

        [Test]
        public void AvailablePackagesShouldFollowCatalogLocations() {
            _folders.Manifests.Add("foo", "name: Foo");
            _folders.Manifests.Add("bar", "name: Bar");
            _folders.Manifests.Add("frap", "name: Frap");
            _folders.Manifests.Add("quad", "name: Quad");

            var available = _manager.AvailablePackages();

            Assert.That(available.Count(), Is.EqualTo(4));
            Assert.That(available, Has.Some.Property("Name").EqualTo("foo"));
        }

        [Test]
        public void PackageDescriptorsShouldHaveNameAndDescription() {

            _folders.Manifests.Add("Sample", @"
name: Sample Package
description: This is the description
version: 2.x
");

            var descriptor = _manager.AvailablePackages().Single();
            Assert.That(descriptor.Name, Is.EqualTo("Sample"));
            Assert.That(descriptor.DisplayName, Is.EqualTo("Sample Package"));
            Assert.That(descriptor.Description, Is.EqualTo("This is the description"));
            Assert.That(descriptor.Version, Is.EqualTo("2.x"));
        }
    }
}
