using System.IO;
using NUnit.Framework;
using Orchard.FileSystems.AppData;
using Orchard.FileSystems.Dependencies;
using Orchard.Tests.FileSystems.AppData;

namespace Orchard.Tests.FileSystems.Dependencies {
    [TestFixture]
    public class DependenciesFolderTests {
        private string _tempFolder;
        private IAppDataFolder _appDataFolder;
        private IDependenciesFolder _dependenciesFolder;

        [SetUp]
        public void Init() {
            _tempFolder = Path.GetTempFileName();
            File.Delete(_tempFolder);

            _appDataFolder = AppDataFolderTests.CreateAppDataFolder(_tempFolder);
            _dependenciesFolder = new DefaultDependenciesFolder(new Stubs.StubCacheManager(), _appDataFolder);
        }

        [TearDown]
        public void Term() {
            Directory.Delete(_tempFolder, true);
        }

        [Test]
        public void LoadDescriptorsShouldReturnEmptyList() {
            Directory.Delete(_tempFolder, true);
            var e = _dependenciesFolder.LoadDescriptors();
            Assert.That(e, Is.Empty);
        }

        [Test]
        public void StoreDescriptors() {
            Directory.Delete(_tempFolder, true);
            var d = new DependencyDescriptor {
                LoaderName = "test",
                Name = "name",
                VirtualPath = "~/bin"
            };
            
            _dependenciesFolder.StoreDescriptors(new [] { d });
            var e = _dependenciesFolder.LoadDescriptors();
            Assert.That(e, Has.Count.EqualTo(1));
        }
    }
}
