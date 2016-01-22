using System;
using System.IO;
using System.Web;
using NUnit.Framework;
using Microsoft.WindowsAzure;
using System.Linq;
using Orchard.Caching;
using Orchard.Environment.Configuration;
using Orchard.Azure.Services.FileSystems.Media;
using Orchard.FileSystems.Media;

namespace Orchard.Azure.Tests.FileSystems.Media {
    [TestFixture]
    public class AzureBlobStorageProviderTests : AzureVirtualEnvironmentTest {

        CloudStorageAccount _devAccount;
        private AzureBlobStorageProvider _azureBlobStorageProvider;

        protected override void OnInit() {
            CloudStorageAccount.TryParse("UseDevelopmentStorage=true", out _devAccount);

            _azureBlobStorageProvider = new AzureBlobStorageProvider(new ShellSettings { Name = "default" }, new ConfigurationMimeTypeProvider(new DefaultCacheManager(typeof(ConfigurationMimeTypeProvider), new DefaultCacheHolder(new DefaultCacheContextAccessor()))));
        }

        [SetUp]
        public void Setup() {
            // ensure default container is empty before running any test
            DeleteAllBlobs(_azureBlobStorageProvider.Container.Name, _devAccount);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetFileShouldOnlyAcceptRelativePath() {
            _azureBlobStorageProvider.CreateFile("foo.txt");
            _azureBlobStorageProvider.GetFile("/foot.txt");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetFileThatDoesNotExistShouldThrow() {
            _azureBlobStorageProvider.GetFile("notexisting");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void DeleteFileThatDoesNotExistShouldThrow() {
            _azureBlobStorageProvider.DeleteFile("notexisting");
        }

        [Test]
        public void RootFolderAreNotCropped() {
            _azureBlobStorageProvider.CreateFolder("default");
            _azureBlobStorageProvider.CreateFolder("foo");

            var folders = _azureBlobStorageProvider.ListFolders("");

            Assert.That(folders.Count(), Is.EqualTo(2));
            Assert.That(folders.Any(f => f.GetName() == "default"), Is.True);
            Assert.That(folders.Any(f => f.GetName() == "foo"), Is.True);
        }

        [Test]
        public void CreateFileShouldReturnCorrectStorageFile() {
            var storageFile = _azureBlobStorageProvider.CreateFile("foo.txt");

            Assert.AreEqual(".txt", storageFile.GetFileType());
            Assert.AreEqual("foo.txt", storageFile.GetName());
            Assert.AreEqual("foo.txt", storageFile.GetPath());
            Assert.AreEqual(0, storageFile.GetSize());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateFileShouldThrowAnExceptionIfAlreadyExisting() {
            var storageFile = _azureBlobStorageProvider.CreateFile("foo.txt");
            Assert.AreEqual(storageFile.GetSize(), 0);

            _azureBlobStorageProvider.CreateFile("foo.txt");
        }

        [Test]
        public void RenameFileShouldCreateANewFileAndRemoveTheOld() {
            _azureBlobStorageProvider.CreateFile("foo1.txt");
            _azureBlobStorageProvider.RenameFile("foo1.txt", "foo2.txt");

            var files = _azureBlobStorageProvider.ListFiles("");

            Assert.AreEqual(1, files.Count());
            Assert.That(files.First().GetPath().Equals("foo2.txt"), Is.True);
            Assert.That(files.First().GetName().Equals("foo2.txt"), Is.True);
        }

        [Test]
        public void CreateFileShouldBeFolderAgnostic() {
            _azureBlobStorageProvider.CreateFile("foo.txt");
            _azureBlobStorageProvider.CreateFile("folder/foo.txt");
            _azureBlobStorageProvider.CreateFile("folder/folder/foo.txt");

            Assert.AreEqual(1, _azureBlobStorageProvider.ListFiles("").Count());
            Assert.AreEqual(1, _azureBlobStorageProvider.ListFiles("folder").Count());
            Assert.AreEqual("folder/foo.txt", _azureBlobStorageProvider.ListFiles("folder").First().GetPath());
            Assert.AreEqual("foo.txt", _azureBlobStorageProvider.ListFiles("folder").First().GetName());
            Assert.AreEqual(1, _azureBlobStorageProvider.ListFiles("folder/folder").Count());
            Assert.AreEqual("folder/folder/foo.txt", _azureBlobStorageProvider.ListFiles("folder/folder").First().GetPath());
            Assert.AreEqual("foo.txt", _azureBlobStorageProvider.ListFiles("folder/folder").First().GetName());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateFolderThatExistsShouldThrow() {
            _azureBlobStorageProvider.CreateFile("folder/foo.txt");
            _azureBlobStorageProvider.CreateFolder("folder");
        }

        [Test]
        public void ListFolderShouldAcceptNullPath() {
            _azureBlobStorageProvider.CreateFolder("folder");
            Assert.AreEqual(1, _azureBlobStorageProvider.ListFolders(null).Count());
            Assert.AreEqual("folder", _azureBlobStorageProvider.ListFolders(null).First().GetName());
            Assert.AreEqual("folder", _azureBlobStorageProvider.ListFolders(null).First().GetPath());
        }

        [Test]
        public void CreateFolderWithSubFolder() {
            _azureBlobStorageProvider.CreateFolder("folder");
            Assert.AreEqual(0, _azureBlobStorageProvider.ListFolders("folder").Count());

            _azureBlobStorageProvider.CreateFolder("folder/folder");
            Assert.AreEqual(1, _azureBlobStorageProvider.ListFolders("folder").Count());
            Assert.AreEqual(0, _azureBlobStorageProvider.ListFiles("folder/folder").Count());
            Assert.AreEqual("folder", _azureBlobStorageProvider.ListFolders("folder").First().GetName());
        }

        [Test]
        public void FoldersShouldBeCreatedRecursively() {
            _azureBlobStorageProvider.CreateFolder("foo/bar/baz");
            Assert.That(_azureBlobStorageProvider.ListFolders("").Count(), Is.EqualTo(1));
            Assert.That(_azureBlobStorageProvider.ListFolders("foo").Count(), Is.EqualTo(1));
            Assert.That(_azureBlobStorageProvider.ListFolders("foo/bar").Count(), Is.EqualTo(1));
        }

        [Test]
        public void ShouldDeleteFiles() {
            _azureBlobStorageProvider.CreateFile("folder/foo1.txt");
            _azureBlobStorageProvider.CreateFile("folder/foo2.txt");
            _azureBlobStorageProvider.CreateFile("folder/folder/foo1.txt");
            _azureBlobStorageProvider.CreateFile("folder/folder/foo2.txt");

            Assert.That(_azureBlobStorageProvider.ListFiles("folder").Count(), Is.EqualTo(2));
            Assert.That(_azureBlobStorageProvider.ListFiles("folder/folder").Count(), Is.EqualTo(2));

            _azureBlobStorageProvider.DeleteFile("folder/foo1.txt");
            _azureBlobStorageProvider.DeleteFile("folder/folder/foo2.txt");

            Assert.That(_azureBlobStorageProvider.ListFiles("folder").Count(), Is.EqualTo(1));
            Assert.That(_azureBlobStorageProvider.ListFiles("folder/folder").Count(), Is.EqualTo(1));
        }

        [Test]
        public void DeleteFolderShouldDeleteFilesAlso() {
            _azureBlobStorageProvider.CreateFile("folder/foo1.txt");
            _azureBlobStorageProvider.CreateFile("folder/foo2.txt");
            _azureBlobStorageProvider.CreateFile("folder/folder/foo1.txt");
            _azureBlobStorageProvider.CreateFile("folder/folder/foo2.txt");

            Assert.AreEqual(2, _azureBlobStorageProvider.ListFiles("folder").Count());
            Assert.AreEqual(2, _azureBlobStorageProvider.ListFiles("folder/folder").Count());

            _azureBlobStorageProvider.DeleteFolder("folder");

            Assert.AreEqual(0, _azureBlobStorageProvider.ListFiles("folder").Count());
            Assert.AreEqual(0, _azureBlobStorageProvider.ListFiles("folder/folder").Count());
        }

        [Test]
        public void ShouldRenameFolders() {
            _azureBlobStorageProvider.CreateFile("folder1/foo.txt");
            _azureBlobStorageProvider.CreateFile("folder2/foo.txt");
            _azureBlobStorageProvider.CreateFile("folder1/folder2/foo.txt");
            _azureBlobStorageProvider.CreateFile("folder1/folder2/folder3/foo.txt");

            Assert.AreEqual(1, _azureBlobStorageProvider.ListFiles("folder1").Count());
            Assert.AreEqual(1, _azureBlobStorageProvider.ListFiles("folder2").Count());
            Assert.AreEqual(1, _azureBlobStorageProvider.ListFiles("folder1/folder2").Count());
            Assert.AreEqual(1, _azureBlobStorageProvider.ListFiles("folder1/folder2/folder3").Count());

            _azureBlobStorageProvider.RenameFolder("folder1/folder2", "folder1/folder4");

            Assert.AreEqual(1, _azureBlobStorageProvider.ListFiles("folder1").Count());
            Assert.AreEqual(1, _azureBlobStorageProvider.ListFiles("folder2").Count());
            Assert.AreEqual(0, _azureBlobStorageProvider.ListFiles("folder1/folder2").Count());
            Assert.AreEqual(1, _azureBlobStorageProvider.ListFiles("folder1/folder4").Count());
            Assert.AreEqual(1, _azureBlobStorageProvider.ListFiles("folder1/folder4/folder3").Count());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotCreateAlreadyExistingFolders() {
            _azureBlobStorageProvider.CreateFile("folder1/foo.txt");
            _azureBlobStorageProvider.CreateFolder("folder1");
        }

        [Test]
        public void TryCreateFolderShouldReturnFalseIfFolderAlreadyExists() {
            _azureBlobStorageProvider.CreateFile("folder1/foo.txt");
            Assert.That(_azureBlobStorageProvider.TryCreateFolder("folder1"), Is.False);
        }

        [Test]
        public void ShouldReadWriteFiles() {
            const string teststring = "This is a test string.";

            var foo = _azureBlobStorageProvider.CreateFile("folder1/foo.txt");

            using ( var stream = foo.OpenWrite() )
            using ( var writer = new StreamWriter(stream) )
                writer.Write(teststring);

            string content;
            using ( var stream = foo.OpenRead() )
            using ( var reader = new StreamReader(stream) ) {
                content = reader.ReadToEnd();
            }

            Assert.AreEqual(teststring, content);
        }

        [Test]
        public void ShouldTruncateFile() {
            var sf = _azureBlobStorageProvider.CreateFile("folder/foo1.txt");
            using (var sw = new StreamWriter(sf.OpenWrite())) {
                sw.Write("foo");
            }

            using (var sw = new StreamWriter(sf.CreateFile())) {
                sw.Write("fo");
            }

            sf = _azureBlobStorageProvider.GetFile("folder/foo1.txt");
            string content;
            using (var sr = new StreamReader(sf.OpenRead())) {
                content = sr.ReadToEnd();
            }
            
            Assert.That(content, Is.EqualTo("fo"));
        }

        [Test]
        public void HttpContextWeaverShouldBeDisposed()
        {
            _azureBlobStorageProvider.CreateFile("foo1.txt");
            _azureBlobStorageProvider.CreateFile("foo2.txt");
            _azureBlobStorageProvider.CreateFile("foo3.txt");

            foreach(var f in _azureBlobStorageProvider.ListFiles(""))
            {
                Assert.That(HttpContext.Current, Is.Null);
            }
        }

        [Test]
        public void MimeTypeShouldBeSet() {
            _azureBlobStorageProvider.CreateFile("foo1.txt");
            var file = _azureBlobStorageProvider.Container.GetBlockBlobReference("default/foo1.txt");
            file.FetchAttributes();
            Assert.That(file.Properties.ContentType, Is.EqualTo("text/plain"));
        }

        [Test]
        public void UnknownMimeTypeShouldBeAssigned() {
            _azureBlobStorageProvider.CreateFile("foo1.xyz");
            var file = _azureBlobStorageProvider.Container.GetBlockBlobReference("default/foo1.xyz");
            file.FetchAttributes();
            Assert.That(file.Properties.ContentType, Is.EqualTo("application/unknown"));
        }


        [Test]
        public void GetStoragePathShouldReturnAValidLocalPath()
        {
            _azureBlobStorageProvider.CreateFile("folder1/foo.txt");
            var publicPath = _azureBlobStorageProvider.GetPublicUrl("folder1/foo.txt");
            var storagePath = _azureBlobStorageProvider.GetStoragePath(publicPath);

            Assert.IsNotNull(storagePath);
            Assert.That(storagePath, Is.EqualTo("folder1/foo.txt"));
        }

        [Test]
        public void GetStoragePathShouldReturnNullIfPathIsNotLocal()
        {
            var storagePath = _azureBlobStorageProvider.GetStoragePath("foo");

            Assert.IsNull(storagePath);
        }

    }
}