using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using Autofac;
using NUnit.Framework;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.WebSite;
using Orchard.Packaging.Services;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Modules.Packaging {
    [TestFixture]
    public class PackageBuilderTests : ContainerTestBase {
        protected override void Register(Autofac.ContainerBuilder builder) {
            builder.RegisterType<PackageBuilder>().As<IPackageBuilder>();
            builder.RegisterType<InMemoryWebSiteFolder>().As<IWebSiteFolder>()
                .As<InMemoryWebSiteFolder>().InstancePerLifetimeScope();
        }
        
        private Stream BuildHelloWorld(IPackageBuilder packageBuilder) {
            return packageBuilder.BuildPackage(new ExtensionDescriptor {
                ExtensionType = "Module",
                Name = "Hello.World",
                Version = "1.0",
                Description = "a",
                Author = "b"
            });
        }
        [Test]
        public void PackageForModuleIsOpcPackage() {
            var packageBuilder = _container.Resolve<IPackageBuilder>();
            var stream = BuildHelloWorld(packageBuilder);

            var package = Package.Open(stream);
            Assert.That(package, Is.Not.Null);
            Assert.That(package.PackageProperties.Identifier, Is.EqualTo("Orchard.Module.Hello.World"));
        }

        [Test]
        public void PropertiesPassThroughAsExpected() {
            var packageBuilder = _container.Resolve<IPackageBuilder>();
            var stream = BuildHelloWorld(packageBuilder);

            var package = Package.Open(stream);
            Assert.That(package.PackageProperties.Description, Is.EqualTo("a"));
            Assert.That(package.PackageProperties.Creator, Is.EqualTo("b"));
            Assert.That(package.PackageProperties.Version, Is.EqualTo("1.0"));
        }


        [Test]
        public void ProjectFileIsAdded() {
            var packageBuilder = _container.Resolve<IPackageBuilder>();
            var folder = _container.Resolve<InMemoryWebSiteFolder>();
            string content;
            using (var sourceStream = GetType().Assembly.GetManifestResourceStream(GetType(), "Hello.World.csproj.txt")) {
                content = new StreamReader(sourceStream).ReadToEnd();
            }
            folder.AddFile("~/Modules/Hello.World/Hello.World.csproj", content);

            var stream = BuildHelloWorld(packageBuilder);

            var package = Package.Open(stream);
            var projectUri = PackUriHelper.CreatePartUri(new Uri("/Content/Modules/Hello.World/Hello.World.csproj", UriKind.Relative));
            var projectPart = package.GetPart(projectUri);
            using (var projectStream = projectPart.GetStream()) {
                var projectContent = new StreamReader(projectStream).ReadToEnd();
                Assert.That(projectContent, Is.EqualTo(content));
            }
        }

    }
}
