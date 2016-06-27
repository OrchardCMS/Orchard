using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;
using NUnit.Framework;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.Media;
using Orchard.Media.Models;
using Orchard.Media.Services;
using Orchard.Tests.Stubs;
using Orchard.Tests.UI.Navigation;

namespace Orchard.Tests.Modules.Media.Services {
    [TestFixture]
    public class MediaServiceTests {
        private const string FolderName1 = "Folder1";
        private const string FolderName2 = "Folder2";
        private const string FolderName3 = "Folder3";

        private const string InnerDirectory = "MyDir";

        private const string TextFileName = "File1.txt";
        private const string PaddedTextFileName = "  File2.txt";
        private const string FinalDottedTextFileName = "file2.txt.";
        private const string WebconfigFileName = "web.config";
        private const string PaddedWebconfigFileName = "  web.config";
        private const string FinalDottedWebconfigFileName = "web.config.";
        private const string DllFileName = "test.dll";
        private const string ZipFileName = "test.zip";
        private const string NoExtensionFileName = "test";

        private const string MediaFolder = "Media";

        private StubOrchardServices OrchardServices { get; set; }
        private StubStorageProvider StorageProvider { get; set; }
        private MediaServiceAccessor MediaService { get; set; }

        [SetUp]
        public void Setup() {
            OrchardServices = new StubOrchardServices();
            StorageProvider = new StubStorageProvider(new ShellSettings { Name = ShellSettings.DefaultName });
            MediaService = new MediaServiceAccessor(StorageProvider, OrchardServices);
        }

        [Test]
        public void GetPublicUrlTests() {
            Assert.That(() => MediaService.GetPublicUrl(null), Throws.InstanceOf(typeof(ArgumentException)), "null relative path is invalid");
            Assert.That(MediaService.GetPublicUrl(TextFileName), Is.EqualTo(string.Format("/{0}/{1}/{2}", MediaFolder, ShellSettings.DefaultName, TextFileName)), "base path file");
            Assert.That(MediaService.GetPublicUrl(string.Format("{0}/{1}", InnerDirectory, TextFileName)), Is.EqualTo(string.Format("/{0}/{1}/{2}/{3}", MediaFolder, ShellSettings.DefaultName, InnerDirectory, TextFileName)), "file within directory");
        }

        [Test]
        public void GetMediaFoldersTest() {
            StorageProvider.ListFoldersPredicate = path => {
                return string.IsNullOrEmpty(path) ? new[] {new StubStorageFolder(FolderName1)}
                            : string.Equals(path, FolderName1) ? new[] {new StubStorageFolder(FolderName2), new StubStorageFolder(FolderName3)}
                                 : new StubStorageFolder[] { };
                };

            IEnumerable<MediaFolder> mediaFolders = MediaService.GetMediaFolders(null);
            Assert.That(mediaFolders.Count(), Is.EqualTo(1), "Root path only has 1 sub directory");
            Assert.That(mediaFolders.FirstOrDefault(mediaFolder => mediaFolder.Name.Equals(FolderName1)), Is.Not.Null, "Correct sub directory in root path");

            mediaFolders = MediaService.GetMediaFolders(FolderName3);
            Assert.That(mediaFolders.Count(), Is.EqualTo(0), "Invalid folder path has 0 sub directories");

            mediaFolders = MediaService.GetMediaFolders(FolderName1);
            Assert.That(mediaFolders.Count(), Is.EqualTo(2), "Folder1 has 2 sub directories");
            Assert.That(mediaFolders.FirstOrDefault(mediaFolder => mediaFolder.Name.Equals(FolderName2)), Is.Not.Null, "Correct sub directory in root path");
            Assert.That(mediaFolders.FirstOrDefault(mediaFolder => mediaFolder.Name.Equals(FolderName3)), Is.Not.Null, "Correct sub directory in root path");
        }

        [Test]
        public void UnzipMediaFileArchiveNotNullParametersTest() {
            // Test basic parameter validation
            Assert.That(() => MediaService.UnzipMediaFileArchiveAccessor(null, new MemoryStream()), Throws.InstanceOf(typeof(ArgumentException)));
            Assert.That(() => MediaService.UnzipMediaFileArchiveAccessor(FolderName1, null), Throws.InstanceOf(typeof(ArgumentException)));
        }

        [Test]
        public void UnzipMediaFileArchiveAdministratorTest() {
            // Test unzip some valid and invalid files as an administrator user
            StorageProvider.SavedStreams.Clear();
            StubWorkContextAccessor.WorkContextImpl.StubSite.DefaultSuperUser = OrchardServices.WorkContext.CurrentUser.UserName;

            MediaService.UnzipMediaFileArchiveAccessor(FolderName1, CreateZipMemoryStream());
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, TextFileName)), Is.True, "text files are allowed for super users");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, PaddedTextFileName)), Is.True, "padded text files are allowed for super users");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, DllFileName)), Is.True, "dll files are allowed for super users");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, ZipFileName)), Is.False, "Recursive zip archive files are not allowed");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, WebconfigFileName)), Is.False, "web.config files are never allowed");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, NoExtensionFileName)), Is.False, "no extension files are never allowed");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, FinalDottedWebconfigFileName)), Is.False, "no extension files are never allowed");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, PaddedWebconfigFileName)), Is.False, "no extension files are never allowed");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, FinalDottedTextFileName)), Is.False, "no extension files are never allowed");
            
            Assert.That(StorageProvider.SavedStreams.Count, Is.EqualTo(3));
        }

        [Test]
        public void UnzipMediaFileArchiveNonAdministratorNoWhitelistTest() {
            // Test unzip some files as a non administrator user and without a white list (everything should be rejected by default)
            StorageProvider.SavedStreams.Clear();
            StubWorkContextAccessor.WorkContextImpl.StubSite.DefaultSuperUser = "myuser";

            MediaService.UnzipMediaFileArchiveAccessor(FolderName1, CreateZipMemoryStream());
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, TextFileName)), Is.False, "text files are not allowed by default for non super users");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, DllFileName)), Is.False, "dll files are not allowed by default for non super users");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, ZipFileName)), Is.False, "Recursive zip archive files are not allowed");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, WebconfigFileName)), Is.False, "web.config files are never allowed");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, NoExtensionFileName)), Is.False, "no extension files are never allowed");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, FinalDottedWebconfigFileName)), Is.False, "no extension files are never allowed");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, PaddedWebconfigFileName)), Is.False, "no extension files are never allowed");
            Assert.That(StorageProvider.SavedStreams.Count, Is.EqualTo(0));
        }

        [Test]
        public void UnzipMediaFileArchiveNonAdministratorWhitelistTest() {
            // Test unzip some files as a non administrator user but with a white list
            StorageProvider.SavedStreams.Clear();
            StubWorkContextAccessor.WorkContextImpl.StubSite.DefaultSuperUser = "myuser";

            MediaSettingsPart mediaSettingsPart = new MediaSettingsPart {
                Record = new MediaSettingsPartRecord { UploadAllowedFileTypeWhitelist = "txt dll config" }
            };

            StubWorkContextAccessor.WorkContextImpl._initMethod = workContext => {
                workContext.CurrentSite.ContentItem.Weld(mediaSettingsPart);
            };

            MediaService.UnzipMediaFileArchiveAccessor(FolderName1, CreateZipMemoryStream());
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, TextFileName)), Is.True, "text files are allowed by the white list for non super users");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, PaddedTextFileName)), Is.True, "padded text files are allowed for super users");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, DllFileName)), Is.True, "dll files are allowed by the white list for non super users");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, ZipFileName)), Is.False, "Recursive zip archive files are not allowed");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, WebconfigFileName)), Is.False, "web.config files are never allowed even if config extensions are");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, NoExtensionFileName)), Is.False, "no extension files are never allowed");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, FinalDottedWebconfigFileName)), Is.False, "no extension files are never allowed");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, FinalDottedTextFileName)), Is.False, "no extension files are never allowed");
            Assert.That(StorageProvider.SavedStreams.Contains(StorageProvider.Combine(FolderName1, PaddedWebconfigFileName)), Is.False, "no extension files are never allowed");
            Assert.That(StorageProvider.SavedStreams.Count, Is.EqualTo(3));
        }

        [Test]
        public void WebConfigIsBlackListed() {
            StorageProvider.SavedStreams.Clear();
            StubWorkContextAccessor.WorkContextImpl.StubSite.DefaultSuperUser = "myuser";

            MediaSettingsPart mediaSettingsPart = new MediaSettingsPart {
                Record = new MediaSettingsPartRecord { UploadAllowedFileTypeWhitelist = "txt dll config" }
            };

            StubWorkContextAccessor.WorkContextImpl._initMethod = workContext => {
                workContext.CurrentSite.ContentItem.Weld(mediaSettingsPart);
            };

            Assert.That(MediaService.FileAllowedAccessor("web.config", true), Is.False);
            Assert.That(MediaService.FileAllowedAccessor("dummy/web.config", true), Is.False);
        }

        private MemoryStream CreateZipMemoryStream() {
            // Setup memory stream with zip archive for more complex scenarios
            MemoryStream memoryStream = new MemoryStream();
            using (ZipFile zipOut = new ZipFile()) {

                zipOut.AddEntry(TextFileName, new byte[] { 0x01 });
                zipOut.AddEntry(WebconfigFileName, new byte[] { 0x02 });
                zipOut.AddEntry(DllFileName, new byte[] { 0x03 });
                zipOut.AddEntry(ZipFileName, new byte[] { 0x04 });
                zipOut.AddEntry(NoExtensionFileName, new byte[] { 0x05 });
                zipOut.AddEntry(PaddedWebconfigFileName, new byte[] { 0x06 });
                zipOut.AddEntry(FinalDottedWebconfigFileName, new byte[] { 0x07 });
                zipOut.AddEntry(PaddedTextFileName, new byte[] { 0x08 });
                zipOut.AddEntry(FinalDottedTextFileName, new byte[] { 0x09 });

                zipOut.Save(memoryStream);
            }
                
            return new MemoryStream(memoryStream.ToArray());
        }

        private class MediaServiceAccessor : MediaService {
            public MediaServiceAccessor(IStorageProvider storageProvider, IOrchardServices orchardServices)
                : base (storageProvider, orchardServices) {}

            public void UnzipMediaFileArchiveAccessor(string targetFolder, Stream zipStream) {
                UnzipMediaFileArchive(targetFolder, zipStream);
            }

            public bool FileAllowedAccessor(string fileName, bool allowZip) {
                return FileAllowed(fileName, allowZip);
            }
        }

        private class StubStorageProvider : IStorageProvider {
            private FileSystemStorageProvider FileSystemStorageProvider { get; set; }
            public Func<string, IEnumerable<IStorageFolder>> ListFoldersPredicate { get; set; }
            public List<string> SavedStreams { get; set; }

            public StubStorageProvider(ShellSettings settings) {
                FileSystemStorageProvider = new FileSystemStorageProvider(settings);
                SavedStreams = new List<string>();
            }

            public bool FileExists(string path) {
                throw new NotImplementedException();
            }

            public string GetPublicUrl(string path) {
                return FileSystemStorageProvider.GetPublicUrl(path);
            }

            public string GetStoragePath(string url) {
                throw new NotImplementedException();
            }

            public IStorageFile GetFile(string path) {
                throw new NotImplementedException();
            }

            public IEnumerable<IStorageFile> ListFiles(string path) {
                throw new NotImplementedException();
            }

            public bool FolderExists(string path) {
                throw new NotImplementedException();
            }

            public IEnumerable<IStorageFolder> ListFolders(string path) {
                return ListFoldersPredicate(path);
            }

            public bool TryCreateFolder(string path) {
                return false;
            }

            public void CreateFolder(string path) {
            }

            public void DeleteFolder(string path) {
            }

            public void RenameFolder(string path, string newPath) {
            }

            public void DeleteFile(string path) {
            }

            public void RenameFile(string path, string newPath) {
            }

            public void CopyFile(string originalPath, string duplicatePath) {
            }

            public IStorageFile CreateFile(string path) {
                throw new NotImplementedException();
            }

            public string Combine(string path1, string path2) {
                return FileSystemStorageProvider.Combine(path1, path2);
            }

            public bool TrySaveStream(string path, Stream inputStream) {
                try { SaveStream(path, inputStream); }
                catch { return false; }

                return true;
            }

            public void SaveStream(string path, Stream inputStream) {
                SavedStreams.Add(path);
            }


            public string GetLocalPath(string url) {
                throw new NotImplementedException();
            }


            public string GetRelativePath(string path) {
                throw new NotImplementedException();
            }
        }

        private class StubStorageFolder : IStorageFolder {
            public string Path { get; set; }
            public string Name { get; set; }

            public StubStorageFolder(string name) {
                Name = name;
            }

            public string GetPath() {
                return Path;
            }

            public string GetName() {
                return Name;
            }

            public long GetSize() {
                return 0;
            }

            public DateTime GetLastUpdated() {
                return DateTime.Now;
            }

            public IStorageFolder GetParent() {
                return new StubStorageFolder("");
            }
        }
    }
}
