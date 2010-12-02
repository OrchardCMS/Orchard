using System.IO;
using Autofac;
using NUnit.Framework;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.VirtualPath;
using Orchard.FileSystems.WebSite;
using Orchard.Packaging.Services;
using Orchard.Services;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Modules.Packaging {
    [TestFixture]
    public class PackageExpanderTests : ContainerTestBase {
        protected override void Register(Autofac.ContainerBuilder builder) {
            builder.RegisterType<PackageBuilder>().As<IPackageBuilder>();
            builder.RegisterType<PackageInstaller>().As<IPackageInstaller>();
            builder.RegisterInstance<IVirtualPathProvider>(new StubVirtualPathProvider(new StubFileSystem(new Clock())));
            builder.RegisterType<InMemoryWebSiteFolder>().As<IWebSiteFolder>()
                .As<InMemoryWebSiteFolder>().InstancePerLifetimeScope();
        }
        
        private Stream BuildHelloWorld(IPackageBuilder packageBuilder) {
            return packageBuilder.BuildPackage(new ExtensionDescriptor {
                ExtensionType = DefaultExtensionTypes.Module,
                Id = "Hello.World",
                Version = "1.0",
                Description = "a",
                Author = "b"
            });
        }
    }
}
