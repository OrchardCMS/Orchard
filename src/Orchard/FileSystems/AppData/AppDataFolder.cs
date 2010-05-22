using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace Orchard.FileSystems.AppData {
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

        public bool FileExists(string path) {
            var filePath = Path.Combine(_basePath, path);
            return File.Exists(filePath);
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