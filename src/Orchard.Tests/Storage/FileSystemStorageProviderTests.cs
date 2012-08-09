using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using System;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.Media;

namespace Orchard.Tests.Storage {
    [TestFixture]
    public class FileSystemStorageProviderTests {

        [SetUp]
        public void Init() {
            _folderPath = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media"), ShellSettings.DefaultName);
            _filePath = _folderPath + "\\testfile.txt";

            if (Directory.Exists(_folderPath)) {
                try {
                    Directory.Delete(_folderPath, true);
                }
                catch {
                    // happens sometimes
                }
            }

            Directory.CreateDirectory(_folderPath);
            File.WriteAllText(_filePath, "testfile contents");

            var subfolder1 = Path.Combine(_folderPath, "Subfolder1");
            Directory.CreateDirectory(subfolder1);
            File.WriteAllText(Path.Combine(subfolder1, "one.txt"), "one contents");
            File.WriteAllText(Path.Combine(subfolder1, "two.txt"), "two contents");

            var subsubfolder1 = Path.Combine(subfolder1, "SubSubfolder1");
            Directory.CreateDirectory(subsubfolder1);

            _storageProvider = new FileSystemStorageProvider(new ShellSettings { Name = ShellSettings.DefaultName });
        }

        [TearDown]
        public void Term() {
            try {
                Directory.Delete(_folderPath, true);
            }
            catch (IOException) {
                // if a system handle is still active give some time to release it
                Thread.Sleep(0);
                Directory.Delete(_folderPath, true);
            }
        }


        private string _filePath;
        private string _folderPath;
        private IStorageProvider _storageProvider;

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetFileThatDoesNotExistShouldThrow() {
            _storageProvider.GetFile("notexisting");
        }

        [Test]
        public void ListFilesShouldReturnFilesFromFilesystem() {
            IEnumerable<IStorageFile> files = _storageProvider.ListFiles(_folderPath);
            Assert.That(files.Count(), Is.EqualTo(1));
        }

        [Test]
        public void ExistingFileIsReturnedWithShortPath() {
            var file = _storageProvider.GetFile("testfile.txt");
            Assert.That(file, Is.Not.Null);
            Assert.That(file.GetPath(), Is.EqualTo("testfile.txt"));
            Assert.That(file.GetName(), Is.EqualTo("testfile.txt"));
        }


        [Test]
        public void ListFilesReturnsItemsWithShortPathAndEnvironmentSlashes() {
            var files = _storageProvider.ListFiles("Subfolder1");
            Assert.That(files, Is.Not.Null);
            Assert.That(files.Count(), Is.EqualTo(2));
            var one = files.Single(x => x.GetName() == "one.txt");
            var two = files.Single(x => x.GetName() == "two.txt");

            Assert.That(one.GetPath(), Is.EqualTo("Subfolder1" + Path.DirectorySeparatorChar + "one.txt"));
            Assert.That(two.GetPath(), Is.EqualTo("Subfolder1" + Path.DirectorySeparatorChar + "two.txt"));
        }


        [Test]
        public void AnySlashInGetFileBecomesEnvironmentAppropriate() {
            var file1 = _storageProvider.GetFile(@"Subfolder1/one.txt");
            var file2 = _storageProvider.GetFile(@"Subfolder1\one.txt");
            Assert.That(file1.GetPath(), Is.EqualTo("Subfolder1" + Path.DirectorySeparatorChar + "one.txt"));
            Assert.That(file2.GetPath(), Is.EqualTo("Subfolder1" + Path.DirectorySeparatorChar + "one.txt"));
        }

        [Test]
        public void ListFoldersReturnsItemsWithShortPathAndEnvironmentSlashes() {
            var folders = _storageProvider.ListFolders(@"Subfolder1");
            Assert.That(folders, Is.Not.Null);
            Assert.That(folders.Count(), Is.EqualTo(1));
            Assert.That(folders.Single().GetName(), Is.EqualTo("SubSubfolder1"));
            Assert.That(folders.Single().GetPath(), Is.EqualTo(Path.Combine("Subfolder1", "SubSubfolder1")));
        }

        [Test]
        public void ParentFolderPathIsStillShort() {
            var subsubfolder = _storageProvider.ListFolders(@"Subfolder1").Single();
            var subfolder = subsubfolder.GetParent();
            Assert.That(subsubfolder.GetName(), Is.EqualTo("SubSubfolder1"));
            Assert.That(subsubfolder.GetPath(), Is.EqualTo(Path.Combine("Subfolder1", "SubSubfolder1")));
            Assert.That(subfolder.GetName(), Is.EqualTo("Subfolder1"));
            Assert.That(subfolder.GetPath(), Is.EqualTo("Subfolder1"));
        }

        [Test]
        public void CreateFolderAndDeleteFolderTakesAnySlash() {
            Assert.That(_storageProvider.ListFolders(@"Subfolder1").Count(), Is.EqualTo(1));
            _storageProvider.CreateFolder(@"SubFolder1/SubSubFolder2");
            _storageProvider.CreateFolder(@"SubFolder1\SubSubFolder3");
            Assert.That(_storageProvider.ListFolders(@"Subfolder1").Count(), Is.EqualTo(3));
            _storageProvider.DeleteFolder(@"SubFolder1/SubSubFolder2");
            _storageProvider.DeleteFolder(@"SubFolder1\SubSubFolder3");
            Assert.That(_storageProvider.ListFolders(@"Subfolder1").Count(), Is.EqualTo(1));
        }

        private IStorageFolder GetFolder(string path) {
            return _storageProvider.ListFolders(Path.GetDirectoryName(path))
                .SingleOrDefault(x => string.Equals(x.GetName(), Path.GetFileName(path), StringComparison.OrdinalIgnoreCase));
        }
        private IStorageFile GetFile(string path) {
            try {
                return _storageProvider.GetFile(path);
            }
            catch (ArgumentException) {
                return null;
            }
        }

        [Test]
        public void ShouldCreateFolders() {
            Directory.Delete(_folderPath, true);
            _storageProvider.CreateFolder("foo/bar/baz");
            Assert.That(_storageProvider.ListFolders("").Count(), Is.EqualTo(1));
            Assert.That(_storageProvider.ListFolders("foo").Count(), Is.EqualTo(1));
            Assert.That(_storageProvider.ListFolders("foo/bar").Count(), Is.EqualTo(1));
        }

        [Test]
        public void RenameFolderTakesShortPathWithAnyKindOfSlash() {
            Assert.That(GetFolder(@"SubFolder1/SubSubFolder1"), Is.Not.Null);
            _storageProvider.RenameFolder(@"SubFolder1\SubSubFolder1", @"SubFolder1/SubSubFolder2");
            _storageProvider.RenameFolder(@"SubFolder1\SubSubFolder2", @"SubFolder1\SubSubFolder3");
            _storageProvider.RenameFolder(@"SubFolder1/SubSubFolder3", @"SubFolder1\SubSubFolder4");
            _storageProvider.RenameFolder(@"SubFolder1/SubSubFolder4", @"SubFolder1/SubSubFolder5");
            Assert.That(GetFolder(Path.Combine("SubFolder1", "SubSubFolder1")), Is.Null);
            Assert.That(GetFolder(Path.Combine("SubFolder1", "SubSubFolder2")), Is.Null);
            Assert.That(GetFolder(Path.Combine("SubFolder1", "SubSubFolder3")), Is.Null);
            Assert.That(GetFolder(Path.Combine("SubFolder1", "SubSubFolder4")), Is.Null);
            Assert.That(GetFolder(Path.Combine("SubFolder1", "SubSubFolder5")), Is.Not.Null);
        }


        [Test]
        public void CreateFileAndDeleteFileTakesAnySlash() {
            Assert.That(_storageProvider.ListFiles(@"Subfolder1").Count(), Is.EqualTo(2));
            var alpha = _storageProvider.CreateFile(@"SubFolder1/alpha.txt");
            var beta = _storageProvider.CreateFile(@"SubFolder1\beta.txt");
            Assert.That(_storageProvider.ListFiles(@"Subfolder1").Count(), Is.EqualTo(4));
            Assert.That(alpha.GetPath(), Is.EqualTo(Path.Combine("SubFolder1", "alpha.txt")));
            Assert.That(beta.GetPath(), Is.EqualTo(Path.Combine("SubFolder1", "beta.txt")));
            _storageProvider.DeleteFile(@"SubFolder1\alpha.txt");
            _storageProvider.DeleteFile(@"SubFolder1/beta.txt");
            Assert.That(_storageProvider.ListFiles(@"Subfolder1").Count(), Is.EqualTo(2));
        }

        [Test]
        public void RenameFileTakesShortPathWithAnyKindOfSlash() {
            Assert.That(GetFile(@"Subfolder1/one.txt"), Is.Not.Null);
            _storageProvider.RenameFile(@"SubFolder1\one.txt", @"SubFolder1/testfile2.txt");
            _storageProvider.RenameFile(@"SubFolder1\testfile2.txt", @"SubFolder1\testfile3.txt");
            _storageProvider.RenameFile(@"SubFolder1/testfile3.txt", @"SubFolder1\testfile4.txt");
            _storageProvider.RenameFile(@"SubFolder1/testfile4.txt", @"SubFolder1/testfile5.txt");
            Assert.That(GetFile(Path.Combine("SubFolder1", "one.txt")), Is.Null);
            Assert.That(GetFile(Path.Combine("SubFolder1", "testfile2.txt")), Is.Null);
            Assert.That(GetFile(Path.Combine("SubFolder1", "testfile3.txt")), Is.Null);
            Assert.That(GetFile(Path.Combine("SubFolder1", "testfile4.txt")), Is.Null);
            Assert.That(GetFile(Path.Combine("SubFolder1", "testfile5.txt")), Is.Not.Null);
        }

        [Test]
        public void GetFileFailsInInvalidPath() {
            Assert.That(() => _storageProvider.GetFile(@"../InvalidFile.txt"), Throws.InstanceOf(typeof(ArgumentException)));
            Assert.That(() => _storageProvider.GetFile(@"../../InvalidFile.txt"), Throws.InstanceOf(typeof(ArgumentException)));

            // Valid get one level up within the storage provider domain
            _storageProvider.CreateFile(@"test.txt");
            Assert.That(_storageProvider.GetFile(@"test.txt"), Is.Not.Null);
            Assert.That(_storageProvider.GetFile(@"SubFolder1\..\test.txt"), Is.Not.Null);
        }

        [Test]
        public void ListFilesFailsInInvalidPath() {
            Assert.That(() => _storageProvider.ListFiles(@"../InvalidFolder"), Throws.InstanceOf(typeof(ArgumentException)));
            Assert.That(() => _storageProvider.ListFiles(@"../../InvalidFolder"), Throws.InstanceOf(typeof(ArgumentException)));

            // Valid get one level up within the storage provider domain
            Assert.That(_storageProvider.ListFiles(@"SubFolder1"), Is.Not.Null);
            Assert.That(_storageProvider.ListFiles(@"SubFolder1\.."), Is.Not.Null);
        }

        [Test]
        public void ListFoldersFailsInInvalidPath() {
            Assert.That(() => _storageProvider.ListFolders(@"../InvalidFolder"), Throws.InstanceOf(typeof(ArgumentException)));
            Assert.That(() => _storageProvider.ListFolders(@"../../InvalidFolder"), Throws.InstanceOf(typeof(ArgumentException)));

            // Valid get one level up within the storage provider domain
            Assert.That(_storageProvider.ListFolders(@"SubFolder1"), Is.Not.Null);
            Assert.That(_storageProvider.ListFolders(@"SubFolder1\.."), Is.Not.Null);
        }

        [Test]
        public void TryCreateFolderFailsInInvalidPath() {
            Assert.That(_storageProvider.TryCreateFolder(@"../InvalidFolder1"), Is.False);
            Assert.That(_storageProvider.TryCreateFolder(@"../../InvalidFolder1"), Is.False);

            // Valid create one level up within the storage provider domain
            Assert.That(_storageProvider.TryCreateFolder(@"SubFolder1\..\ValidFolder1"), Is.True);
        }

        [Test]
        public void CreateFolderFailsInInvalidPath() {
            Assert.That(() => _storageProvider.CreateFolder(@"../InvalidFolder1"), Throws.InstanceOf(typeof(ArgumentException)));
            Assert.That(() => _storageProvider.CreateFolder(@"../../InvalidFolder1"), Throws.InstanceOf(typeof(ArgumentException)));

            // Valid create one level up within the storage provider domain
            _storageProvider.CreateFolder(@"SubFolder1\..\ValidFolder1");
            Assert.That(GetFolder("ValidFolder1"), Is.Not.Null);
        }

        [Test]
        public void DeleteFolderFailsInInvalidPath() {
            Assert.That(() => _storageProvider.DeleteFolder(@"../InvalidFolder1"), Throws.InstanceOf(typeof(ArgumentException)));
            Assert.That(() => _storageProvider.DeleteFolder(@"../../InvalidFolder1"), Throws.InstanceOf(typeof(ArgumentException)));

            // Valid create one level up within the storage provider domain
            Assert.That(GetFolder("SubFolder1"), Is.Not.Null);
            _storageProvider.DeleteFolder(@"SubFolder1\..\SubFolder1");
            Assert.That(GetFolder("SubFolder1"), Is.Null);
        }

        [Test]
        public void RenameFolderFailsInInvalidPath() {
            Assert.That(GetFolder(@"SubFolder1/SubSubFolder1"), Is.Not.Null);
            Assert.That(() => _storageProvider.RenameFolder(@"SubFolder1", @"../SubSubFolder1"), Throws.InstanceOf(typeof(ArgumentException)));
            Assert.That(() => _storageProvider.RenameFolder(@"SubFolder1", @"../../SubSubFolder1"), Throws.InstanceOf(typeof(ArgumentException)));

            // Valid move one level up within the storage provider domain
            _storageProvider.RenameFolder(@"SubFolder1\SubSubFolder1", @"SubFolder1\..\SubSubFolder1");
            Assert.That(GetFolder("SubSubFolder1"), Is.Not.Null);

            _storageProvider.CreateFolder(@"SubFolder1\SubSubFolder1\SubSubSubFolder1");
            _storageProvider.RenameFolder(@"SubFolder1\SubSubFolder1\SubSubSubFolder1", @"SubFolder1\SubSubFolder1\..\SubSubSubFolder1");
            Assert.That(GetFolder(@"SubFolder1\SubSubSubFolder1"), Is.Not.Null);
        }

        [Test]
        public void DeleteFileFailsInInvalidPath() {
            Assert.That(() => _storageProvider.DeleteFile(@"../test.txt"), Throws.InstanceOf(typeof(ArgumentException)));
            Assert.That(() => _storageProvider.DeleteFile(@"../test.txt"), Throws.InstanceOf(typeof(ArgumentException)));

            // Valid move one level up within the storage provider domain
            _storageProvider.CreateFile(@"test.txt");
            Assert.That(GetFile("test.txt"), Is.Not.Null);
            _storageProvider.DeleteFile(@"test.txt");
            Assert.That(GetFile("test.txt"), Is.Null);

            _storageProvider.CreateFile(@"test.txt");
            Assert.That(GetFile("test.txt"), Is.Not.Null);
            _storageProvider.DeleteFile(@"SubFolder1\..\test.txt");
            Assert.That(GetFile("test.txt"), Is.Null);
        }

        [Test]
        public void RenameFileFailsInInvalidPath() {
            Assert.That(() => _storageProvider.RenameFile(@"../test.txt", "invalid.txt"), Throws.InstanceOf(typeof(ArgumentException)));
            Assert.That(() => _storageProvider.RenameFile(@"../test.txt", "invalid.txt"), Throws.InstanceOf(typeof(ArgumentException)));

            // Valid move one level up within the storage provider domain
            _storageProvider.CreateFile(@"test.txt");
            Assert.That(GetFile("test.txt"), Is.Not.Null);
            _storageProvider.RenameFile(@"test.txt", "newName.txt");
            Assert.That(GetFile("newName.txt"), Is.Not.Null);
            _storageProvider.RenameFile(@"SubFolder1\..\newName.txt", "newNewName.txt");
            Assert.That(GetFile("newNewName.txt"), Is.Not.Null);
        }

        [Test]
        public void CreateFileFailsInInvalidPath() {
            Assert.That(() => _storageProvider.CreateFile(@"../InvalidFolder1.txt"), Throws.InstanceOf(typeof(ArgumentException)));
            Assert.That(() => _storageProvider.CreateFile(@"../../InvalidFolder1.txt"), Throws.InstanceOf(typeof(ArgumentException)));

            // Valid create one level up within the storage provider domain
            _storageProvider.CreateFile(@"SubFolder1\..\ValidFolder1.txt");
            Assert.That(GetFile("ValidFolder1.txt"), Is.Not.Null);
        }

        [Test]
        public void SaveStreamFailsInInvalidPath() {
            _storageProvider.CreateFile(@"test.txt");

            using (Stream stream = GetFile("test.txt").OpenRead()) {
                Assert.That(() => _storageProvider.SaveStream(@"../newTest.txt", stream), Throws.InstanceOf(typeof(ArgumentException)));
                Assert.That(() => _storageProvider.SaveStream(@"../../newTest.txt", stream), Throws.InstanceOf(typeof(ArgumentException)));

                // Valid create one level up within the storage provider domain
                _storageProvider.SaveStream(@"SubFolder1\..\newTest.txt", stream);
                Assert.That(GetFile("newTest.txt"), Is.Not.Null);
            }
        }

        [Test]
        public void TrySaveStreamFailsInInvalidPath() {
            _storageProvider.CreateFile(@"test.txt");

            using (Stream stream = GetFile("test.txt").OpenRead()) {
                Assert.That(_storageProvider.TrySaveStream(@"../newTest.txt", stream), Is.False);
                Assert.That(_storageProvider.TrySaveStream(@"../../newTest.txt", stream), Is.False);

                // Valid create one level up within the storage provider domain
                Assert.That(_storageProvider.TrySaveStream(@"SubFolder1\..\newTest.txt", stream), Is.True);
            }
        }
    }
}
