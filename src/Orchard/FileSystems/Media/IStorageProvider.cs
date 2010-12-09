using System.Collections.Generic;

namespace Orchard.FileSystems.Media {
    public interface IStorageProvider : IDependency {
        string GetPublicUrl(string path);
        IStorageFile GetFile(string path);
        IEnumerable<IStorageFile> ListFiles(string path);
        IEnumerable<IStorageFolder> ListFolders(string path);
        void TryCreateFolder(string path);
        void CreateFolder(string path);
        void DeleteFolder(string path);
        void RenameFolder(string path, string newPath);
        void DeleteFile(string path);
        void RenameFile(string path, string newPath);
        IStorageFile CreateFile(string path);
        string Combine(string path1, string path2);
    }
}