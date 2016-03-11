using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Ionic.Zip;
using Orchard.ContentManagement;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.Media.Models;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Validation;

namespace Orchard.Media.Services {
    /// <summary>
    /// The MediaService class provides the services to manipulate media entities (files / folders).
    /// Among other things it provides filtering functionalities on file types.
    /// The actual manipulation of the files is, however, delegated to the IStorageProvider.
    /// </summary>
    public class MediaService : IMediaService {
        private readonly IStorageProvider _storageProvider;
        private readonly IOrchardServices _orchardServices;

        /// <summary>
        /// Initializes a new instance of the MediaService class with a given IStorageProvider and IOrchardServices.
        /// </summary>
        /// <param name="storageProvider">The storage provider.</param>
        /// <param name="orchardServices">The orchard services provider.</param>
        public MediaService(IStorageProvider storageProvider, IOrchardServices orchardServices) {
            _storageProvider = storageProvider;
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        /// <summary>
        /// Retrieves the public path based on the relative path within the media directory.
        /// </summary>
        /// <example>
        /// "/Media/Default/InnerDirectory/Test.txt" based on the input "InnerDirectory/Test.txt"
        /// </example>
        /// <param name="relativePath">The relative path within the media directory.</param>
        /// <returns>The public path relative to the application url.</returns>
        public string GetPublicUrl(string relativePath) {
            Argument.ThrowIfNullOrEmpty(relativePath, "relativePath");

            return _storageProvider.GetPublicUrl(relativePath);
        }

        /// <summary>
        /// Returns the public URL for a media file.
        /// </summary>
        /// <param name="mediaPath">The relative path of the media folder containing the media.</param>
        /// <param name="fileName">The media file name.</param>
        /// <returns>The public URL for the media.</returns>
        public string GetMediaPublicUrl(string mediaPath, string fileName) {
            return GetPublicUrl(Path.Combine(mediaPath, fileName));
        }

        /// <summary>
        /// Retrieves the media folders within a given relative path.
        /// </summary>
        /// <param name="relativePath">The path where to retrieve the media folder from. null means root.</param>
        /// <returns>The media folder in the given path.</returns>
        public IEnumerable<MediaFolder> GetMediaFolders(string relativePath) {
            return _storageProvider.ListFolders(relativePath).Select(folder =>
                new MediaFolder {
                    Name = folder.GetName(),
                    Size = folder.GetSize(),
                    LastUpdated = folder.GetLastUpdated(),
                    MediaPath = folder.GetPath()
                }).ToList();
        }

        /// <summary>
        /// Retrieves the media files within a given relative path.
        /// </summary>
        /// <param name="relativePath">The path where to retrieve the media files from. null means root.</param>
        /// <returns>The media files in the given path.</returns>
        public IEnumerable<MediaFile> GetMediaFiles(string relativePath) {
            return _storageProvider.ListFiles(relativePath).Select(file =>
                new MediaFile {
                    Name = file.GetName(),
                    Size = file.GetSize(),
                    LastUpdated = file.GetLastUpdated(),
                    Type = file.GetFileType(),
                    FolderName = relativePath,
                    MediaPath = GetMediaPublicUrl(relativePath, file.GetName())
                }).ToList();
        }

        /// <summary>
        /// Creates a media folder.
        /// </summary>
        /// <param name="relativePath">The path where to create the new folder. null means root.</param>
        /// <param name="folderName">The name of the folder to be created.</param>
        public void CreateFolder(string relativePath, string folderName) {
            Argument.ThrowIfNullOrEmpty(folderName, "folderName");

            _storageProvider.CreateFolder(relativePath == null ? folderName : _storageProvider.Combine(relativePath, folderName));
        }

        /// <summary>
        /// Deletes a media folder.
        /// </summary>
        /// <param name="folderPath">The path to the folder to be deleted.</param>
        public void DeleteFolder(string folderPath) {
            Argument.ThrowIfNullOrEmpty(folderPath, "folderPath");

            _storageProvider.DeleteFolder(folderPath);
        }

        /// <summary>
        /// Renames a media folder.
        /// </summary>
        /// <param name="folderPath">The path to the folder to be renamed.</param>
        /// <param name="newFolderName">The new folder name.</param>
        public void RenameFolder(string folderPath, string newFolderName) {
            Argument.ThrowIfNullOrEmpty(folderPath, "folderPath");
            Argument.ThrowIfNullOrEmpty(newFolderName, "newFolderName");

            _storageProvider.RenameFolder(folderPath, _storageProvider.Combine(Path.GetDirectoryName(folderPath), newFolderName));
        }

        /// <summary>
        /// Deletes a media file.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="fileName">The file name.</param>
        public void DeleteFile(string folderPath, string fileName) {
            Argument.ThrowIfNullOrEmpty(folderPath, "folderPath");
            Argument.ThrowIfNullOrEmpty(fileName, "fileName");

            _storageProvider.DeleteFile(_storageProvider.Combine(folderPath, fileName));
        }

        /// <summary>
        /// Renames a media file.
        /// </summary>
        /// <param name="folderPath">The path to the file's parent folder.</param>
        /// <param name="currentFileName">The current file name.</param>
        /// <param name="newFileName">The new file name.</param>
        public void RenameFile(string folderPath, string currentFileName, string newFileName) {
            Argument.ThrowIfNullOrEmpty(folderPath, "folderPath");
            Argument.ThrowIfNullOrEmpty(currentFileName, "currentFileName");
            Argument.ThrowIfNullOrEmpty(newFileName, "newFileName");

            if (!FileAllowed(newFileName, false)) {
                if (string.IsNullOrEmpty(Path.GetExtension(newFileName))) {
                    throw new ArgumentException(T("New file name \"{0}\" is not allowed. Please provide a file extension.", newFileName).ToString());
                }
                throw new ArgumentException(T("New file name \"{0}\" is not allowed.", newFileName).ToString());
            }

            _storageProvider.RenameFile(_storageProvider.Combine(folderPath, currentFileName), _storageProvider.Combine(folderPath, newFileName));
        }

        /// <summary>
        /// Moves a media file.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="currentPath">The path to the file's parent folder.</param>
        /// <param name="newPath">The path where the file will be moved to.</param>
        public void MoveFile(string fileName, string currentPath, string newPath) {
            Argument.ThrowIfNullOrEmpty(currentPath, "currentPath");
            Argument.ThrowIfNullOrEmpty(newPath, "newPath");
            Argument.ThrowIfNullOrEmpty(fileName, "fileName");

            _storageProvider.RenameFile(_storageProvider.Combine(currentPath, fileName), _storageProvider.Combine(newPath, fileName));
        }

        /// <summary>
        /// Uploads a media file based on a posted file.
        /// </summary>
        /// <param name="folderPath">The path to the folder where to upload the file.</param>
        /// <param name="postedFile">The file to upload.</param>
        /// <param name="extractZip">Boolean value indicating weather zip files should be extracted.</param>
        /// <returns>The path to the uploaded file.</returns>
        public string UploadMediaFile(string folderPath, HttpPostedFileBase postedFile, bool extractZip) {
            Argument.ThrowIfNullOrEmpty(folderPath, "folderPath");
            Argument.ThrowIfNull(postedFile, "postedFile");

            return UploadMediaFile(folderPath, Path.GetFileName(postedFile.FileName), postedFile.InputStream, extractZip);
        }

        /// <summary>
        /// Uploads a media file based on an array of bytes.
        /// </summary>
        /// <param name="folderPath">The path to the folder where to upload the file.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="bytes">The array of bytes with the file's contents.</param>
        /// <param name="extractZip">Boolean value indicating weather zip files should be extracted.</param>
        /// <returns>The path to the uploaded file.</returns>
        public string UploadMediaFile(string folderPath, string fileName, byte [] bytes, bool extractZip) {
            Argument.ThrowIfNullOrEmpty(folderPath, "folderPath");
            Argument.ThrowIfNullOrEmpty(fileName, "fileName");
            Argument.ThrowIfNull(bytes, "bytes");

            return UploadMediaFile(folderPath, fileName, new MemoryStream(bytes), extractZip);
        }

        /// <summary>
        /// Uploads a media file based on a stream.
        /// </summary>
        /// <param name="folderPath">The folder path to where to upload the file.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="inputStream">The stream with the file's contents.</param>
        /// <param name="extractZip">Boolean value indicating weather zip files should be extracted.</param>
        /// <returns>The path to the uploaded file.</returns>
        public string UploadMediaFile(string folderPath, string fileName, Stream inputStream, bool extractZip) {
            Argument.ThrowIfNullOrEmpty(folderPath, "folderPath");
            Argument.ThrowIfNullOrEmpty(fileName, "fileName");
            Argument.ThrowIfNull(inputStream, "inputStream");

            if (extractZip && IsZipFile(Path.GetExtension(fileName))) {
                UnzipMediaFileArchive(folderPath, inputStream);

                // Don't save the zip file.
                return _storageProvider.GetPublicUrl(folderPath);
            }

            if (!FileAllowed(fileName, true)) {
                var currentSite = _orchardServices.WorkContext.CurrentSite;
                var mediaSettings = currentSite.As<MediaSettingsPart>();

                throw new ArgumentException(T("Could not upload file {0}. Supported file types are {1}.", fileName, mediaSettings.UploadAllowedFileTypeWhitelist).Text);
            }

            string filePath = _storageProvider.Combine(folderPath, fileName);
            _storageProvider.SaveStream(filePath, inputStream);

            return _storageProvider.GetPublicUrl(filePath);
        }

        /// <summary>
        /// Verifies if a file is allowed based on its name and the policies defined by the black / white lists.
        /// </summary>
        /// <param name="postedFile">The posted file</param>
        /// <returns>True if the file is allowed; false if otherwise.</returns>
        public bool FileAllowed(HttpPostedFileBase postedFile) {
            if (postedFile == null) {
                return false;
            }

            return FileAllowed(postedFile.FileName, true);
        }

        /// <summary>
        /// Verifies if a file is allowed based on its name and the policies defined by the black / white lists.
        /// </summary>
        /// <param name="fileName">The file name of the file to validate.</param>
        /// <param name="allowZip">Boolean value indicating weather zip files are allowed.</param>
        /// <returns>True if the file is allowed; false if otherwise.</returns>
        public bool FileAllowed(string fileName, bool allowZip) {
            string localFileName = GetFileName(fileName);
            string extension = GetExtension(localFileName);
            if (string.IsNullOrEmpty(localFileName) || string.IsNullOrEmpty(extension)) {
                return false;
            }

            ISite currentSite = _orchardServices.WorkContext.CurrentSite;
            IUser currentUser = _orchardServices.WorkContext.CurrentUser;

            // zip files at the top level are allowed since this is how you upload multiple files at once.
            if (IsZipFile(extension)) {
                return allowZip;
            }

            // whitelist does not apply to the superuser
            if (currentUser == null || !currentSite.SuperUser.Equals(currentUser.UserName, StringComparison.Ordinal)) {

                // must be in the whitelist
                MediaSettingsPart mediaSettings = currentSite.As<MediaSettingsPart>();
                
                if (mediaSettings == null) {
                    return false;
                } 
                
                if(String.IsNullOrWhiteSpace(mediaSettings.UploadAllowedFileTypeWhitelist)) {
                    return true;
                } 

                if(!mediaSettings.UploadAllowedFileTypeWhitelist.ToUpperInvariant().Split(' ').Contains(extension.ToUpperInvariant())) {
                    return false;
                }
            }

            // blacklist always applies
            if (string.Equals(localFileName, "web.config", StringComparison.OrdinalIgnoreCase)) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Unzips a media archive file.
        /// </summary>
        /// <param name="targetFolder">The folder where to unzip the file.</param>
        /// <param name="zipStream">The archive file stream.</param>
        protected void UnzipMediaFileArchive(string targetFolder, Stream zipStream) {
            Argument.ThrowIfNullOrEmpty(targetFolder, "targetFolder");
            Argument.ThrowIfNull(zipStream, "zipStream");

            using (var fileInflater = ZipFile.Read(zipStream)) {
                // We want to preserve whatever directory structure the zip file contained instead
                // of flattening it.
                // The API below doesn't necessarily return the entries in the zip file in any order.
                // That means the files in subdirectories can be returned as entries from the stream 
                // before the directories that contain them, so we create directories as soon as first
                // file below their path is encountered.
                foreach (ZipEntry entry in fileInflater) {
                    if (entry == null) {
                        continue;
                    }

                    if (!entry.IsDirectory && !string.IsNullOrEmpty(entry.FileName)) {

                        // skip disallowed files
                        if (FileAllowed(entry.FileName, false)) {
                            string fullFileName = _storageProvider.Combine(targetFolder, entry.FileName);

                            using (var stream = entry.OpenReader()) {
                                // the call will return false if the file already exists
                                if (!_storageProvider.TrySaveStream(fullFileName, stream)) {
                                    
                                    // try to delete the file and save again
                                    try {
                                        _storageProvider.DeleteFile(fullFileName);
                                        _storageProvider.TrySaveStream(fullFileName, stream);
                                    }
                                    catch (ArgumentException) {
                                        // ignore the exception as the file doesn't exist
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines if a file is a Zip Archive based on its extension.
        /// </summary>
        /// <param name="extension">The extension of the file to analyze.</param>
        /// <returns>True if the file is a Zip archive; false otherwise.</returns>
        private static bool IsZipFile(string extension) {
            return string.Equals(extension.TrimStart('.'), "zip", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetFileName(string fileName) {
            return Path.GetFileName(fileName).Trim();
        }

        private static string GetExtension(string fileName) {
            return Path.GetExtension(fileName).Trim().TrimStart('.');
        }
    }
}
