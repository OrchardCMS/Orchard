using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using System;
using Orchard.Storage;

namespace Orchard.Tests.Storage {
    [TestFixture]
    public class FileSystemStorageProviderTests {
        #region Setup/Teardown

        [TestFixtureSetUp]
        public void InitFixture() {
            _folderPath = Directory.CreateDirectory(Path.GetTempPath() + "filesystemstorageprovidertests").FullName;
            _filePath = _folderPath + "\\testfile.txt";
            FileStream fileStream = File.Create(_filePath);
            fileStream.Close();
            _fileSystemStorageProvider = new FileSystemStorageProvider();
        }

        [TestFixtureTearDown]
        public void TermFixture() {
            Directory.Delete(_folderPath, true);
        }

        #endregion

        private string _filePath;
        private string _folderPath;
        private FileSystemStorageProvider _fileSystemStorageProvider;

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetFileThatDoesNotExistShouldThrow() {
            _fileSystemStorageProvider.GetFile("notexisting");
        }

        [Test]
        public void ListFilesShouldReturnFilesFromFilesystem() {
            IEnumerable<IStorageFile> files = _fileSystemStorageProvider.ListFiles(_folderPath);
            Assert.That(files.Count(), Is.EqualTo(1));
        }
    }
}
