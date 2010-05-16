using System.Collections.Generic;
using Orchard.Caching;

namespace Orchard.FileSystems.AppData {
    /// <summary>
    /// Abstraction of App_Data folder
    /// Expected to work on physical filesystem, but decouples core
    /// system from web hosting apis
    /// </summary>
    public interface IAppDataFolder : IVolatileProvider {
        IEnumerable<string> ListFiles(string path);
        IEnumerable<string> ListDirectories(string path);

        void CreateFile(string path, string content);
        string ReadFile(string path);
        void DeleteFile(string path);
        bool FileExists(string path);

        string CreateDirectory(string path);


        /// <summary>
        /// May be called to initialize component when not in a hosting environment
        /// app domain
        /// </summary>
        void SetBasePath(string basePath);
        string MapPath(string path);
    }
}