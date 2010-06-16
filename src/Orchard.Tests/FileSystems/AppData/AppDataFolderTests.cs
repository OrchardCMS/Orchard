using System.IO;
using System.Linq;
using NUnit.Framework;
using Orchard.FileSystems.AppData;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.FileSystems.AppData {
    [TestFixture]
    public class AppDataFolderTests {
        private string _tempFolder;
        private IAppDataFolder _appDataFolder;

        public class StubAppDataFolderRoot : IAppDataFolderRoot {
            public string RootPath { get; set; }
            public string RootFolder { get; set; }
        }

        public static IAppDataFolder CreateAppDataFolder(string tempFolder) {
            var folderRoot = new StubAppDataFolderRoot {RootPath = "~/App_Data", RootFolder = tempFolder};
            var monitor = new StubVirtualPathMonitor();
            return new AppDataFolder(folderRoot, monitor);
        }

        [SetUp]
        public void Init() {
            _tempFolder = Path.GetTempFileName();
            File.Delete(_tempFolder);
            Directory.CreateDirectory(Path.Combine(_tempFolder, "alpha"));
            File.WriteAllText(Path.Combine(_tempFolder, "alpha\\beta.txt"), "beta-content");
            File.WriteAllText(Path.Combine(_tempFolder, "alpha\\gamma.txt"), "gamma-content");
            Directory.CreateDirectory(Path.Combine(_tempFolder, "alpha\\omega"));

            _appDataFolder = CreateAppDataFolder(_tempFolder);
        }

        [TearDown]
        public void Term() {
            Directory.Delete(_tempFolder, true);
        }

        [Test]
        public void ListFilesShouldContainSubPathAndFileName() {
            var files = _appDataFolder.ListFiles("alpha");
            Assert.That(files.Count(), Is.EqualTo(2));
            Assert.That(files, Has.Some.EqualTo("alpha/beta.txt"));
            Assert.That(files, Has.Some.EqualTo("alpha/gamma.txt"));
        }

        [Test]
        public void NonExistantFolderShouldListAsEmptyCollection() {
            var files = _appDataFolder.ListFiles("delta");
            Assert.That(files.Count(), Is.EqualTo(0));
        }

        [Test]
        public void PhysicalPathAddsToBasePathAndDoesNotNeedToExist() {
            var physicalPath = _appDataFolder.MapPath("delta\\epsilon.txt");
            Assert.That(physicalPath, Is.EqualTo(Path.Combine(_tempFolder, "delta\\epsilon.txt")));
        }

        [Test]
        public void ListSubdirectoriesShouldContainFullSubpath() {
            var files = _appDataFolder.ListDirectories("alpha");
            Assert.That(files.Count(), Is.EqualTo(1));
            Assert.That(files, Has.Some.EqualTo("alpha/omega"));
        }

        [Test]
        public void ListSubdirectoriesShouldWorkInRoot() {
            var files = _appDataFolder.ListDirectories("");
            Assert.That(files.Count(), Is.EqualTo(1));
            Assert.That(files, Has.Some.EqualTo("alpha"));
        }


        [Test]
        public void NonExistantFolderShouldListDirectoriesAsEmptyCollection() {
            var files = _appDataFolder.ListDirectories("delta");
            Assert.That(files.Count(), Is.EqualTo(0));
        }

        [Test]
        public void CreateFileWillCauseDirectoryToBeCreated() {
            Assert.That(Directory.Exists(Path.Combine(_tempFolder, "alpha\\omega\\foo")), Is.False);
            _appDataFolder.CreateFile("alpha\\omega\\foo\\bar.txt", "quux");
            Assert.That(Directory.Exists(Path.Combine(_tempFolder, "alpha\\omega\\foo")), Is.True);
        }


        [Test]
        public void FilesCanBeReadBack() {            
            _appDataFolder.CreateFile("alpha\\gamma\\foo\\bar.txt", @"
this is
a
test");
            var text = _appDataFolder.ReadFile("alpha\\gamma\\foo\\bar.txt");
            Assert.That(text, Is.EqualTo(@"
this is
a
test"));
        }

        [Test]
        public void FileExistsReturnsFalseForNonExistingFile() {
            Assert.That(_appDataFolder.FileExists("notexisting"), Is.False);
        }

        [Test]
        public void FileExistsReturnsTrueForExistingFile() {
            _appDataFolder.CreateFile("alpha\\foo\\bar.txt", "");
            Assert.That(_appDataFolder.FileExists("alpha\\foo\\bar.txt"), Is.True);
        }
    }
}
