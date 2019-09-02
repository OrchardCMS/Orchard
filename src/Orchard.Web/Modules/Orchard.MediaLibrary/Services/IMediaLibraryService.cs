using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.MediaLibrary.Factories;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Services {
    public interface IMediaLibraryService : IDependency {
        IEnumerable<ContentTypeDefinition> GetMediaTypes();
        IContentQuery<MediaPart, MediaPartRecord> GetMediaContentItems(VersionOptions versionOptions = null);
        IEnumerable<MediaPart> GetMediaContentItems(string folderPath, int skip, int count, string order, string mediaType, VersionOptions versionOptions = null);
        IEnumerable<MediaPart> GetMediaContentItems(int skip, int count, string order, string mediaType, VersionOptions versionOptions = null);
        IEnumerable<MediaPart> GetMediaContentItemsRecursive(string folderPath, int skip, int count, string order, string mediaType, VersionOptions versionOptions = null);
        int GetMediaContentItemsCount(string folderPath, string mediaType, VersionOptions versionOptions = null);
        int GetMediaContentItemsCount(string mediaType, VersionOptions versionOptions = null);
        int GetMediaContentItemsCountRecursive(string folderPath, string mediaType, VersionOptions versionOptions = null);
        MediaPart ImportMedia(string relativePath, string filename);
        MediaPart ImportMedia(string relativePath, string filename, string contentType);
        MediaPart ImportMedia(Stream stream, string relativePath, string filename);
        MediaPart ImportMedia(Stream stream, string relativePath, string filename, string contentType);
        IMediaFactory GetMediaFactory(Stream stream, string mimeType, string contentType);
        bool CheckMediaFolderPermission(Orchard.Security.Permissions.Permission permission, string folderPath);
        /// <summary>
        /// Creates a unique filename to prevent filename collisions.
        /// </summary>
        /// <param name="folderPath">The relative where collisions will be checked.</param>
        /// <param name="filename">The desired filename.</param>
        /// <returns>A string representing a unique filename.</returns>
        string GetUniqueFilename(string folderPath, string filename);

        /// <summary>
        /// Returns the public URL for a media file.
        /// </summary>
        /// <param name="mediaPath">The relative path of the media folder containing the media.</param>
        /// <param name="fileName">The media file name.</param>
        /// <returns>The public URL for the media.</returns>
        string GetMediaPublicUrl(string mediaPath, string fileName);

        IMediaFolder GetRootMediaFolder();

        IMediaFolder GetUserMediaFolder();

        /// <summary>
        /// Retrieves the media folders within a given relative path.
        /// </summary>
        /// <param name="relativePath">The path where to retrieve the media folder from. null means root.</param>
        /// <returns>The media folder in the given path.</returns>
        IEnumerable<IMediaFolder> GetMediaFolders(string relativePath);

        /// <summary>
        /// Retrieves the media files within a given relative path.
        /// </summary>
        /// <param name="relativePath">The path where to retrieve the media files from. null means root.</param>
        /// <returns>The media files in the given path.</returns>
        IEnumerable<MediaFile> GetMediaFiles(string relativePath);

        /// <summary>
        /// Creates a media folder.
        /// </summary>
        /// <param name="relativePath">The path where to create the new folder. null means root.</param>
        /// <param name="folderName">The name of the folder to be created.</param>
        void CreateFolder(string relativePath, string folderName);

        /// <summary>
        /// Deletes a media folder.
        /// </summary>
        /// <param name="folderPath">The path to the folder to be deleted.</param>
        void DeleteFolder(string folderPath);

        /// <summary>
        /// Renames a media folder.
        /// </summary>
        /// <param name="folderPath">The path to the folder to be renamed.</param>
        /// <param name="newFolderName">The new folder name.</param>
        void RenameFolder(string folderPath, string newFolderName);

        /// <summary>
        /// Deletes a media file.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="fileName">The file name.</param>
        void DeleteFile(string folderPath, string fileName);

        /// <summary>
        /// Renames a media file.
        /// </summary>
        /// <param name="folderPath">The path to the file's parent folder.</param>
        /// <param name="currentFileName">The current file name.</param>
        /// <param name="newFileName">The new file name.</param>
        void RenameFile(string folderPath, string currentFileName, string newFileName);

        /// <summary>
        /// Moves a media file.
        /// </summary>
        /// <param name="currentPath">The path to the file's parent folder.</param>
        /// <param name="filename">The file name.</param>
        /// <param name="newPath">The path where the file will be moved to.</param>
        /// <param name="newFilename">The new file name.</param>
        void MoveFile(string currentPath, string filename, string newPath, string newFilename);

        /// <summary>
        /// Moves a media file.
        /// </summary>
        /// <param name="currentPath">The path to the file's parent folder.</param>
        /// <param name="filename">The file name.</param>
        /// <param name="duplicatePath">The path where the file will be copied to.</param>
        /// <param name="duplicateFilename">The new file name.</param>
        void CopyFile(string currentPath, string filename, string duplicatePath, string duplicateFilename);

        /// <summary>
        /// Uploads a media file based on a posted file.
        /// </summary>
        /// <param name="folderPath">The path to the folder where to upload the file.</param>
        /// <param name="postedFile">The file to upload.</param>
        /// <returns>The path to the uploaded file.</returns>
        string UploadMediaFile(string folderPath, HttpPostedFileBase postedFile);

        /// <summary>
        /// Uploads a media file based on an array of bytes.
        /// </summary>
        /// <param name="folderPath">The path to the folder where to upload the file.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="bytes">The array of bytes with the file's contents.</param>
        /// <returns>The path to the uploaded file.</returns>
        string UploadMediaFile(string folderPath, string fileName, byte[] bytes);

        /// <summary>
        /// Uploads a media file based on a stream.
        /// </summary>
        /// <param name="folderPath">The folder path to where to upload the file.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="inputStream">The stream with the file's contents.</param>
        /// <returns>The path to the uploaded file.</returns>
        string UploadMediaFile(string folderPath, string fileName, Stream inputStream);

        /// <summary>
        /// Combines two paths.
        /// </summary>
        /// <param name="path1">The parent path.</param>
        /// <param name="path2">The child path.</param>
        /// <returns>The combined path.</returns>
        string Combine(string path1, string path2);
    }

    public static class MediaLibraryServiceExtensions {
        public static bool CanManageMediaFolder(this IMediaLibraryService service, string folderPath) {
            // The current user can manage a media if he has access to the whole hierarchy
            // or the media is under his personal storage folder.

            var rootMediaFolder = service.GetRootMediaFolder();
            if (rootMediaFolder == null) {
                return true;
            }

            var mediaPath = service.Combine(folderPath, " ").Trim();
            var rootPath = service.Combine(rootMediaFolder.MediaPath, " ").Trim();

            return mediaPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase);
        }

        public static string GetRootedFolderPath(this IMediaLibraryService service, string folderPath) {
            var rootMediaFolder = service.GetRootMediaFolder();
            if (rootMediaFolder != null) {
                return service.Combine(rootMediaFolder.MediaPath, folderPath ?? "");
            }

            return folderPath;
        }

        public static string MimeTypeToContentType(this IMediaLibraryService service, Stream stream, string mimeType, string contentType) {
            var mediaFactory = service.GetMediaFactory(stream, mimeType, contentType);
            if (mediaFactory == null)
                return null;

            switch (mediaFactory.GetType().Name) {
                case "ImageFactory":
                    return "Image";
                case "AudioFactory":
                    return "Audio";
                case "DocumentFactory":
                    return "Document";
                case "VectorImageFactory":
                    return "VectorImage";
                case "VideoFactory":
                    return "Video";
                default:
                    return null;
            }
        }
    }
}