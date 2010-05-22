using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using ICSharpCode.SharpZipLib.Zip;
using JetBrains.Annotations;
using Orchard.FileSystems.Media;
using Orchard.Logging;
using Orchard.Media.Models;

namespace Orchard.Media.Services {
    [UsedImplicitly]
    public class MediaService : IMediaService {
        private readonly IStorageProvider _storageProvider;

        public MediaService(
            IStorageProvider storageProvider) {
            _storageProvider = storageProvider;
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
                    MediaPath = folder.GetPath(),
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
            _storageProvider.CreateFolder(mediaPath + "\\" + name);
        }

        public void DeleteFolder(string name) {
            _storageProvider.DeleteFolder(name);
        }

        public void RenameFolder(string path, string newName) {
            var newPath = RenameFolderPath(path, newName);
            _storageProvider.RenameFolder(path, newPath);
        }

        public void DeleteFile(string name, string folderName) {
            _storageProvider.DeleteFile(folderName + "\\" + name);
        }

        public void RenameFile(string name, string newName, string folderName) {
            _storageProvider.RenameFile(folderName + "\\" + name, folderName + "\\" + newName);
        }

        public string UploadMediaFile(string folderName, HttpPostedFileBase postedFile) {

            if (postedFile.FileName.EndsWith(".zip")) {
                UnzipMediaFileArchive(folderName, postedFile);
                // Don't save the zip file.
                return _storageProvider.GetPublicUrl(folderName);
            }
            
            if (postedFile.ContentLength > 0) {
                var filePath = Path.Combine(folderName, Path.GetFileName(postedFile.FileName));
                var inputStream = postedFile.InputStream;

                SaveStream(filePath, inputStream);
                return _storageProvider.GetPublicUrl(filePath);
            }

            return null;
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

                        try { _storageProvider.CreateFolder(directoryName); }
                        catch {
                            // no handling needed - this is to force the folder to exist if it doesn't
                        }

                        SaveStream(entryName, fileInflater);
                    }
                }
            }
        }

        private static string RenameFolderPath(string path, string newName) {
            var lastIndex = path.LastIndexOf("\\");

            if (lastIndex == -1) {
                return newName;
            }

            return path.Substring(0, lastIndex) + "\\" + newName;
        }
    }
}
