using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using ICSharpCode.SharpZipLib.Zip;
using JetBrains.Annotations;
using Orchard.Logging;
using Orchard.Storage;
using Orchard.Media.Models;

namespace Orchard.Media.Services {
    [UsedImplicitly]
    public class MediaService : IMediaService {
        private readonly IStorageProvider _storageProvider;
        private readonly string _rootPath;

        public MediaService (
            IStorageProvider storageProvider) {
            _storageProvider = storageProvider;
            _rootPath = HttpContext.Current.Server.MapPath("~/Media");
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public IEnumerable<MediaFolder> GetMediaFolders(string path) {
            var mediaFolders = new List<MediaFolder>();
            var folders = (
                path == null ? 
                _storageProvider.ListFolders(_rootPath) :
                _storageProvider.ListFolders(_rootPath + "\\" + path));

            foreach (var folder in folders) {
                var parentHierarchy = GetParentHierarchy(folder);
                var mediaPath = GetMediaPath(parentHierarchy, folder.GetName());
                var mediaFolder = new MediaFolder {
                    Name = folder.GetName(),
                    Size = folder.GetSize(),
                    LastUpdated = folder.GetLastUpdated(),
                    MediaPath = mediaPath
                };
                mediaFolders.Add(mediaFolder);
            }
            return mediaFolders;
        }

        public IEnumerable<MediaFile> GetMediaFiles(string path) {
            var mediaFiles = new List<MediaFile>();

            var files = _storageProvider.ListFiles(_rootPath + "\\" + path);
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
                _storageProvider.CreateFolder(_rootPath + "\\" + name);
                return;
            }
            _storageProvider.CreateFolder(_rootPath + "\\" + mediaPath + "\\" + name);
        }

        public void DeleteFolder(string name) {
            _storageProvider.DeleteFolder(_rootPath + "\\" + name);
        }

        public void RenameFolder(string path, string newName) {
            var newPath = RenameFolderPath(path, newName);
            _storageProvider.RenameFolder(_rootPath + "\\" + path, _rootPath + "\\" + newPath);
        }

        public void DeleteFile(string name, string folderName) {
            _storageProvider.DeleteFile(_rootPath + "\\" + folderName + "\\" + name);
        }

        public void RenameFile(string name, string newName, string folderName) {
            _storageProvider.RenameFile(_rootPath + "\\" + folderName + "\\" + name, _rootPath + "\\" + folderName + "\\" + newName);
        }

        public void UploadMediaFile(string folderName, HttpPostedFileBase postedFile) {
            var targetFolder = HttpContext.Current.Server.MapPath("~/Media/" + folderName);
            if (postedFile.FileName.EndsWith(".zip")) {
                UnzipMediaFileArchive(targetFolder, postedFile);
                // Don't save the zip file.
                return;
            }
            if (postedFile.ContentLength > 0) {
                string filePath = Path.Combine(targetFolder,
                                               Path.GetFileName(postedFile.FileName));
                postedFile.SaveAs(filePath);
            }
        }

        private static void UnzipMediaFileArchive(string targetFolder, HttpPostedFileBase postedFile) {
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
                    var directoryName = Path.GetDirectoryName(entry.Name);
                    if (!Directory.Exists(Path.Combine(targetFolder, directoryName))) {
                        Directory.CreateDirectory(Path.Combine(targetFolder, directoryName));
                    }

                    if (!entry.IsDirectory && entry.Name.Length > 0) {
                        var len = Convert.ToInt32(entry.Size);
                        var extractedBytes = new byte[len];
                        fileInflater.Read(extractedBytes, 0, len);
                        File.WriteAllBytes(Path.Combine(targetFolder, entry.Name), extractedBytes);
                    }
                }
            }
        }

        private static List<string> GetParentHierarchy(IStorageFolder folder) {
            var parentHierarchy = new List<string>();
            do {
                IStorageFolder parentFolder = folder.GetParent();
                string parentName = parentFolder.GetName();
                if (String.Equals(parentName, "Media", StringComparison.OrdinalIgnoreCase)) {
                    break;
                }
                parentHierarchy.Insert(0, parentName);
                folder = parentFolder;
            } while (true);
            
            return parentHierarchy;
        }

        private static string GetMediaPath(IEnumerable<string> parentHierarchy, string folderName) {
            var mediaPath = new StringBuilder();
            foreach (string parent in parentHierarchy) {
                mediaPath.Append(parent);
                mediaPath.Append("\\");
            }
            mediaPath.Append(folderName);
            return mediaPath.ToString();
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
