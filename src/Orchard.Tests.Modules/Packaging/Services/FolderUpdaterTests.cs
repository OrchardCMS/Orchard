using System.IO;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Packaging.Services;
using Orchard.UI.Notify;

namespace Orchard.Tests.Modules.Packaging.Services {
    [TestFixture]
    public class FolderUpdaterTests {
        protected IContainer _container;

        private readonly string _basePath = Path.Combine(Path.GetTempPath(), "FolderUpdaterTests");

        [SetUp]
        public virtual void Init() {
            if (Directory.Exists(_basePath)) {
                Directory.Delete(_basePath, true);
            }

            Directory.CreateDirectory(_basePath);

            var builder = new ContainerBuilder();

            builder.RegisterType<FolderUpdater>().As<IFolderUpdater>();
            builder.RegisterInstance(new Mock<INotifier>().Object);

            _container = builder.Build();
        }

        [TestFixtureTearDown]
        public void Clean() {
            if (Directory.Exists(_basePath)) {
                Directory.Delete(_basePath, true);
            }
        }

        [Test]
        public void BackupTest() {
            DirectoryInfo sourceDirectoryInfo = Directory.CreateDirectory(Path.Combine(_basePath, "Source"));
            File.CreateText(Path.Combine(sourceDirectoryInfo.FullName, "file1.txt")).Close();
            File.CreateText(Path.Combine(sourceDirectoryInfo.FullName, "file2.txt")).Close();

            IFolderUpdater folderUpdater = _container.Resolve<IFolderUpdater>();

            DirectoryInfo targetDirectoryInfo = new DirectoryInfo(Path.Combine(_basePath, "Target"));
            folderUpdater.Backup(sourceDirectoryInfo, targetDirectoryInfo);

            Assert.That(File.Exists(Path.Combine(targetDirectoryInfo.FullName, "file1.txt")), Is.True);
            Assert.That(File.Exists(Path.Combine(targetDirectoryInfo.FullName, "file2.txt")), Is.True);
        }
    }
}
