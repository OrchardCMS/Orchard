using System.Collections.Generic;

namespace Orchard.Storage {
    public interface IStorageProvider : IDependency {
        IStorageFile GetFile(string path);
        IEnumerable<IStorageFile> ListFiles(string path);
        IEnumerable<IStorageFolder> ListFolders(string path);
        void CreateFolder(string path);
        void DeleteFolder(string path);
        void RenameFolder(string path, string newPath);
        void DeleteFile(string path);
        void RenameFile(string path, string newPath);
        IStorageFile CreateFile(string path);

        /// <summary>
        /// Combines two path strings
        /// </summary>
        /// <param name="path1">The first path</param>
        /// <param name="path2">The second path</param>
        /// <returns>A string containing the combined paths. If one of the specified paths is a zero-length string, this method returns the other path. 
        /// If path2 contains an absolute path, this method returns path2.</returns>
        string Combine(string path1, string path2);
    }
}
