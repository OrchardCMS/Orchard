using System.Collections.Generic;
using System.Web;
using Orchard.Media.Models;

namespace Orchard.Media.Services {
    public interface IMediaService : IDependency {
        IEnumerable<MediaFolder> GetMediaFolders(string path);
        IEnumerable<MediaFile> GetMediaFiles(string path);
        void CreateFolder(string path, string name);
        void DeleteFolder(string name);
        void RenameFolder(string path, string newName);
        void DeleteFile(string name, string folderName);
        void RenameFile(string name, string newName, string folderName);
        void UploadMediaFile(string folderName, HttpPostedFileBase postedFile);
    }
}