using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace Orchard.Environment.Configuration {
    /// <summary>
    /// Abstraction of App_Data folder
    /// Expected to work on physical filesystem, but decouples core
    /// system from web hosting apis
    /// </summary>
    public interface IAppDataFolder {
        IEnumerable<string> ListFiles(string path);
        IEnumerable<string> ListDirectories(string path);

        void CreateFile(string path, string content);
        string ReadFile(string path);
        void DeleteFile(string path);

        string CreateDirectory(string path);


        /// <summary>
        /// May be called to initialize component when not in a hosting environment
        /// app domain
        /// </summary>
        void SetBasePath(string basePath);
        string MapPath(string path);
    }

    public class AppDataFolder : IAppDataFolder {
        protected string _basePath;

        public AppDataFolder() {
            _basePath = HostingEnvironment.MapPath("~/App_Data");
        }

        public void CreateFile(string path, string content) {
            var filePath = Path.Combine(_basePath, path);
            var folderPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            File.WriteAllText(filePath, content);
        }

        public string ReadFile(string path) {
            return File.ReadAllText(Path.Combine(_basePath, path));
        }

        public void DeleteFile(string path) {
            File.Delete(Path.Combine(_basePath, path));
        }

        public IEnumerable<string> ListFiles(string path) {
            var directoryPath = Path.Combine(_basePath, path);
            if (!Directory.Exists(directoryPath))
                return Enumerable.Empty<string>();

            var files = Directory.GetFiles(directoryPath);

            return files.Select(file => {
                var fileName = Path.GetFileName(file);
                return Path.Combine(path, fileName);
            });
        }

        public IEnumerable<string> ListDirectories(string path) {
            var directoryPath = Path.Combine(_basePath, path);
            if (!Directory.Exists(directoryPath))
                return Enumerable.Empty<string>();

            var files = Directory.GetDirectories(directoryPath);

            return files.Select(file => {
                var fileName = Path.GetFileName(file);
                return Path.Combine(path, fileName);
            });
        }

        public string CreateDirectory(string path) {
            var directory = Path.Combine(_basePath, path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return directory;
        }

        public void SetBasePath(string basePath) {
            _basePath = basePath;
        }

        public string MapPath(string path) {
            return Path.Combine(_basePath, path);
        }
    }
}
