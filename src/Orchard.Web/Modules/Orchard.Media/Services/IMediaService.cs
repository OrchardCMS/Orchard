using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Orchard.Media.Models;

namespace Orchard.Media.Services {
    public interface IMediaService : IDependency {
        /// <summary>
        /// Retrieves the public path based on the relative path within the media directory.
        /// </summary>
        /// <example>
        /// "/Media/Default/InnerDirectory/Test.txt" based on the input "InnerDirectory/Test.txt"
        /// </example>
        /// <param name="relativePath">The relative path within the media directory.</param>
        /// <returns>The public path relative to the application url.</returns>
        string GetPublicUrl(string relativePath);

        /// <summary>
        /// Returns the public URL for a media file.
        /// </summary>
        /// <param name="mediaPath">The relative path of the media folder containing the media.</param>
        /// <param name="fileName">The media file name.</param>
        /// <returns>The public URL for the media.</returns>
        string GetMediaPublicUrl(string mediaPath, string fileName);

        /// <summary>
        /// Retrieves the media folders within a given relative path.
        /// </summary>
        /// <param name="relativePath">The path where to retrieve the media folder from. null means root.</param>
        /// <returns>The media folder in the given path.</returns>
        IEnumerable<MediaFolder> GetMediaFolders(string relativePath);

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
        /// Uploads a media file based on a posted file.
        /// </summary>
        /// <param name="folderPath">The path to the folder where to upload the file.</param>
        /// <param name="postedFile">The file to upload.</param>
        /// <param name="extractZip">Boolean value indicating weather zip files should be extracted.</param>
        /// <returns>The path to the uploaded file.</returns>
        string UploadMediaFile(string folderPath, HttpPostedFileBase postedFile, bool extractZip);

        /// <summary>
        /// Uploads a media file based on an array of bytes.
        /// </summary>
        /// <param name="folderPath">The path to the folder where to upload the file.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="bytes">The array of bytes with the file's contents.</param>
        /// <param name="extractZip">Boolean value indicating weather zip files should be extracted.</param>
        /// <returns>The path to the uploaded file.</returns>
        string UploadMediaFile(string folderPath, string fileName, byte[] bytes, bool extractZip);

        /// <summary>
        /// Uploads a media file based on a stream.
        /// </summary>
        /// <param name="folderPath">The folder path to where to upload the file.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="inputStream">The stream with the file's contents.</param>
        /// <param name="extractZip">Boolean value indicating weather zip files should be extracted.</param>
        /// <returns>The path to the uploaded file.</returns>
        string UploadMediaFile(string folderPath, string fileName, Stream inputStream, bool extractZip);

        /// <summary>
        /// Verifies if a file is allowed based on its name and the policies defined by the black / white lists.
        /// </summary>
        /// <param name="postedFile">The posted file</param>
        /// <returns>True if the file is allowed; false if otherwise.</returns>
        bool FileAllowed(HttpPostedFileBase postedFile);

        /// <summary>
        /// Verifies if a file is allowed based on its name and the policies defined by the black / white lists.
        /// </summary>
        /// <param name="fileName">The file name of the file to validate.</param>
        /// <param name="allowZip">Boolean value indicating weather zip files are allowed.</param>
        /// <returns>True if the file is allowed; false if otherwise.</returns>
        bool FileAllowed(string fileName, bool allowZip);
    }
}