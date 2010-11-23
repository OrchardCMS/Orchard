using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using ICSharpCode.SharpZipLib.Zip;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.FileSystems.Media;
using Orchard.Logging;
using Orchard.Media.Models;

namespace Orchard.Media.Services {
    [UsedImplicitly]
    public class MediaService : IMediaService {
        private readonly IStorageProvider _storageProvider;
        private readonly IOrchardServices _orchardServices;

        public MediaService(IStorageProvider storageProvider, IOrchardServices orchardServices) {
            _storageProvider = storageProvider;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public string GetPublicUrl(string path) {
            return _storageProvider.GetPublicUrl(path);
        }

        public IEnumerable<MediaFolder> GetMediaFolders(string path) {
            var mediaFolders = new List<MediaFolder>();
            var folders = _storageProvider.ListFolders(path);

            foreach (var folder in folders) {
                var mediaFolder = new MediaFolder {
                    Name = folder.GetName(),
                    Size = folder.GetSize(),
                    LastUpdated = folder.GetLastUpdated(),
                    MediaPath = folder.GetPath()
                };
                mediaFolders.Add(mediaFolder);
            }
            return mediaFolders;
        }

        public IEnumerable<MediaFile> GetMediaFiles(string path) {
            var mediaFiles = new List<MediaFile>();

            var files = _storageProvider.ListFiles(path);
            foreach (var file in files) {
                var mediaFile = new MediaFile {
                    Name = file.GetName(),
                    Size = file.GetSize(),
                    LastUpdated = file.GetLastUpdated(),
                    Type = file.GetFileType(),
                    FolderName = path
                };
                mediaFiles.Add(mediaFile);
            }
            return mediaFiles;
        }

        //TODO: Use Path.Combine.
        public void CreateFolder(string mediaPath, string name) {
            if (String.IsNullOrEmpty(mediaPath)) {
                _storageProvider.CreateFolder(name);
                return;
            }
            _storageProvider.CreateFolder(_storageProvider.Combine(mediaPath, name));
        }

        public void DeleteFolder(string name) {
            _storageProvider.DeleteFolder(name);
        }

        public void RenameFolder(string path, string newName) {
            var newPath = RenameFolderPath(path, newName);
            _storageProvider.RenameFolder(path, newPath);
        }

        public void DeleteFile(string name, string folderName) {
            _storageProvider.DeleteFile(_storageProvider.Combine(folderName, name));
        }

        public void RenameFile(string name, string newName, string folderName) {
            if (FileAllowed(newName, false)) {
                _storageProvider.RenameFile(_storageProvider.Combine(folderName, name), _storageProvider.Combine(folderName, newName));
            }
        }

        public string UploadMediaFile(string folderName, HttpPostedFileBase postedFile) {
            if (postedFile.FileName.EndsWith(".zip")) {
                UnzipMediaFileArchive(folderName, postedFile);
                // Don't save the zip file.
                return _storageProvider.GetPublicUrl(folderName);
            }
            if (FileAllowed(postedFile) && postedFile.ContentLength > 0) {
                var filePath = Path.Combine(folderName, Path.GetFileName(postedFile.FileName));
                var inputStream = postedFile.InputStream;

                SaveStream(filePath, inputStream);
                return _storageProvider.GetPublicUrl(filePath);
            }

            return null;
        }

        private bool FileAllowed(string name, bool allowZip) {
            if (string.IsNullOrWhiteSpace(name)) {
                return false;
            }
            var currentSite = _orchardServices.WorkContext.CurrentSite;
            var mediaSettings = currentSite.As<MediaSettingsPart>();
            var allowedExtensions = mediaSettings.UploadAllowedFileTypeWhitelist.ToUpperInvariant().Split(' ');
            var ext = (Path.GetExtension(name) ?? "").TrimStart('.').ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(ext)) {
                return false;
            }
            // whitelist does not apply to the superuser
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser == null || !currentSite.SuperUser.Equals(currentUser.UserName, StringComparison.Ordinal)) {
                // zip files at the top level are allowed since this is how you upload multiple files at once.
                if (allowZip && ext.Equals("zip", StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
                // must be in the whitelist
                if (Array.IndexOf(allowedExtensions, ext) == -1) {
                    return false;
                }
            }
            // blacklist always applies
            if (string.Equals(name.Trim(), "web.config", StringComparison.OrdinalIgnoreCase)) {
                return false;
            }
            return true;
        }

        public bool FileAllowed(HttpPostedFileBase postedFile) {
            if (postedFile == null) {
                return false;
            }
            return FileAllowed(postedFile.FileName, true);
        }

        private void SaveStream(string filePath, Stream inputStream) {
            var file = _storageProvider.CreateFile(filePath);
            var outputStream = file.OpenWrite();
            var buffer = new byte[8192];
            for (; ; ) {

                var length = inputStream.Read(buffer, 0, buffer.Length);
                if (length <= 0)
                    break;
                outputStream.Write(buffer, 0, length);
            }
            outputStream.Dispose();
        }

        private void UnzipMediaFileArchive(string targetFolder, HttpPostedFileBase postedFile) {
            var postedFileLength = postedFile.ContentLength;
            var postedFileStream = postedFile.InputStream;
            var postedFileData = new byte[postedFileLength];
            postedFileStream.Read(postedFileData, 0, postedFileLength);

            using (var memoryStream = new MemoryStream(postedFileData)) {
                var fileInflater = new ZipInputStream(memoryStream);
                ZipEntry entry;
                // We want to preserve whatever directory structure the zip file contained instead
                // of flattening it.
                // The API below doesn't necessarily return the entries in the zip file in any order.
                // That means the files in subdirectories can be returned as entries from the stream 
                // before the directories that contain them, so we create directories as soon as first
                // file below their path is encountered.
                while ((entry = fileInflater.GetNextEntry()) != null) {

                    if (!entry.IsDirectory && entry.Name.Length > 0) {
                        var entryName = Path.Combine(targetFolder, entry.Name);
                        var directoryName = Path.GetDirectoryName(entryName);

                        // skip disallowed files
                        if (FileAllowed(entry.Name, false)) {
                            try {
                                _storageProvider.CreateFolder(directoryName);
                            }
                            catch {
                                // no handling needed - this is to force the folder to exist if it doesn't
                            }

                            SaveStream(entryName, fileInflater);
                        }
                    }
                }
            }
        }

        private string RenameFolderPath(string path, string newName) {
            var lastIndex = Math.Max(path.LastIndexOf(Path.DirectorySeparatorChar), path.LastIndexOf(Path.AltDirectorySeparatorChar));

            if (lastIndex == -1) {
                return newName;
            }

            return _storageProvider.Combine(path.Substring(0, lastIndex), newName);
        }
    }
}
