using System.Collections.Generic;
using System.Web;
using Orchard.Media.Models;

namespace Orchard.Media.Services {
    public interface IMediaService : IDependency {
        string GetPublicUrl(string path);
        IEnumerable<MediaFolder> GetMediaFolders(string path);
        IEnumerable<MediaFile> GetMediaFiles(string path);
        void CreateFolder(string path, string name);
        void DeleteFolder(string name);
        void RenameFolder(string path, string newName);
        void DeleteFile(string name, string folderName);
        void RenameFile(string name, string newName, string folderName);
        string UploadMediaFile(string folderName, string fileName, byte[] bytes, bool extractZip);
        string UploadMediaFile(string folderName, HttpPostedFileBase postedFile, bool extractZip);
        bool FileAllowed(HttpPostedFileBase postedFile);
    }
}