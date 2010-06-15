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
            if (Directory.Exists(_tempFolder))
                Directory.Delete(_tempFolder, true);
            var e = _dependenciesFolder.LoadDescriptors();
            Assert.That(e, Is.Empty);
        }

        [Test]
        public void StoreDescriptorsShouldWork() {
            if (Directory.Exists(_tempFolder))
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

        [Test]
        public void StoreDescriptorsShouldNoOpIfNoChanges() {
            if (Directory.Exists(_tempFolder))
                Directory.Delete(_tempFolder, true);
            var d1 = new DependencyDescriptor {
                LoaderName = "test1",
                Name = "name1",
                VirtualPath = "~/bin1"
            };

            var d2 = new DependencyDescriptor {
                LoaderName = "test2",
                Name = "name2",
                VirtualPath = "~/bin2"
            };

            _dependenciesFolder.StoreDescriptors(new[] { d1, d2 });
            var dateTime1 = File.GetLastWriteTimeUtc(Path.Combine(_tempFolder, "Dependencies", "Dependencies.xml"));

            _dependenciesFolder.StoreDescriptors(new[] { d2, d1 });
            var dateTime2 = File.GetLastWriteTimeUtc(Path.Combine(_tempFolder, "Dependencies", "Dependencies.xml"));
            Assert.That(dateTime1, Is.EqualTo(dateTime2));
            
        }
    }
}
