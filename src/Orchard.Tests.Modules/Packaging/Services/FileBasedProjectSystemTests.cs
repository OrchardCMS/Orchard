using System.IO;
using Autofac;
using Moq;
using NuGet;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.VirtualPath;
using Orchard.FileSystems.WebSite;
using Orchard.Packaging.Services;
using Orchard.Services;
using Orchard.Tests.Stubs;
using Orchard.UI.Notify;
using IPackageBuilder = Orchard.Packaging.Services.IPackageBuilder;
using PackageBuilder = Orchard.Packaging.Services.PackageBuilder;

namespace Orchard.Tests.Modules.Packaging.Services {
    [TestFixture]
    public class FileBasedProjectSystemTests : ContainerTestBase {
        private const string PackageIdentifier = "Hello.World";

        private readonly string _basePath = Path.Combine(Path.GetTempPath(), "PackageInstallerTests");

        private Mock<IVirtualPathProvider> _mockedVirtualPathProvider;

        protected override void Register(ContainerBuilder builder) {
            builder.RegisterType<PackageBuilder>().As<IPackageBuilder>();
            builder.RegisterType<PackageInstaller>().As<IPackageInstaller>();
            builder.RegisterType<ExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<FolderUpdater>().As<IFolderUpdater>();
            builder.RegisterInstance(new Mock<INotifier>().Object);
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();
            builder.RegisterType<StubAsyncTokenProvider>().As<IAsyncTokenProvider>();

            _mockedVirtualPathProvider = new Mock<IVirtualPathProvider>();
            builder.RegisterInstance(_mockedVirtualPathProvider.Object).As<IVirtualPathProvider>();
            builder.RegisterType<DefaultOrchardFrameworkAssemblies>().As<IOrchardFrameworkAssemblies>();
            builder.RegisterType<InMemoryWebSiteFolder>().As<IWebSiteFolder>()
                .As<InMemoryWebSiteFolder>().InstancePerLifetimeScope();
            builder.RegisterType<StubClock>().As<IClock>();
        }

        [SetUp]
        public override void Init() {
            base.Init();

            if (Directory.Exists(_basePath)) {
                Directory.Delete(_basePath, true);
            }

            Directory.CreateDirectory(_basePath);
        }

        [TestFixtureTearDown]
        public void Clean() {
            if (Directory.Exists(_basePath)) {
                Directory.Delete(_basePath, true);
            }
        }

        private Stream BuildHelloWorld(IPackageBuilder packageBuilder) {
            // add some content because NuGet requires it
            var folder = _container.Resolve<InMemoryWebSiteFolder>();
            using (var sourceStream = GetType().Assembly.GetManifestResourceStream(GetType(), "Hello.World.csproj.txt")) {
                folder.AddFile("~/Modules/Hello.World/Hello.World.csproj", new StreamReader(sourceStream).ReadToEnd());
            }

            return packageBuilder.BuildPackage(new ExtensionDescriptor {
                ExtensionType = DefaultExtensionTypes.Module,
                Id = PackageIdentifier,
                Version = "1.0",
                Description = "a",
                Author = "b"
            });
        }

        [Test]
        public void ValidPathsTest() {
            IPackageBuilder packageBuilder = _container.Resolve<IPackageBuilder>();
            Stream stream = BuildHelloWorld(packageBuilder);

            string filename = Path.Combine(_basePath, "package.nupkg");
            using (var fileStream = File.Create(filename)) {
                stream.CopyTo(fileStream);
            }

            ZipPackage zipPackage = new ZipPackage(filename);
            IPackageInstaller packageInstaller = _container.Resolve<IPackageInstaller>();

            _mockedVirtualPathProvider.Setup(v => v.MapPath(It.IsAny<string>()))
                .Returns<string>(path => Path.Combine(_basePath, path.Replace("~\\", "")));

            _mockedVirtualPathProvider.Setup(v => v.Combine(It.IsAny<string[]>()))
                .Returns<string[]>(Path.Combine);

            PackageInfo packageInfo = packageInstaller.Install(zipPackage, _basePath, _basePath);
            Assert.That(packageInfo, Is.Not.Null);
            Assert.That(Directory.Exists(Path.Combine(_basePath, "Modules/Hello.World")));
            Assert.That(File.Exists(Path.Combine(_basePath, "Modules/Hello.World/Hello.World.csproj")));
            Assert.That(!File.Exists(Path.Combine(_basePath, "Modules/Hello.World/Service%References/SomeReference.cs")));
            Assert.That(File.Exists(Path.Combine(_basePath, "Modules/Hello.World/Service References/SomeReference.cs")));
        }
    }
}
