using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Orchard.Environment.Configuration;

namespace Orchard.Tests.Environment.Configuration {
    [TestFixture]
    public class AppDataFolderTests {
        private string _tempFolder;
        private IAppDataFolder _appDataFolder;

        [SetUp]
        public void Init() {
            _tempFolder = Path.GetTempFileName();
            File.Delete(_tempFolder);
            Directory.CreateDirectory(Path.Combine(_tempFolder, "alpha"));
            File.WriteAllText(Path.Combine(_tempFolder, "alpha\\beta.txt"), "beta-content");
            File.WriteAllText(Path.Combine(_tempFolder, "alpha\\gamma.txt"), "gamma-content");

            _appDataFolder = new AppDataFolder();
            _appDataFolder.SetBasePath(_tempFolder);
        }

        [TearDown]
        public void Term() {
            Directory.Delete(_tempFolder, true);
        }

        [Test]
        public void ListFilesShouldContainSubPathAndFileName() {
            var files = _appDataFolder.ListFiles("alpha");
            Assert.That(files.Count(), Is.EqualTo(2));
            Assert.That(files, Has.Some.EqualTo("alpha\\beta.txt"));
            Assert.That(files, Has.Some.EqualTo("alpha\\gamma.txt"));
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

    }
}
