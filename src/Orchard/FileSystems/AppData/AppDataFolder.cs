using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Orchard.Caching;
using Orchard.Environment;
using Orchard.FileSystems.VirtualPath;

namespace Orchard.FileSystems.AppData {
    public class AppDataFolder : IAppDataFolder {
        private readonly IVirtualPathMonitor _virtualPathMonitor;
        protected string _basePath;

        public AppDataFolder(IVirtualPathMonitor virtualPathMonitor) {
            _virtualPathMonitor = virtualPathMonitor;
            _basePath = HostingEnvironment.MapPath("~/App_Data");
        }

        /// <summary>
        /// Combine a set of virtual paths into a physical path based at "_basePath"
        /// </summary>
        private string CombineToPhysicalPath(params string[] paths) {
            return Path.Combine(_basePath, Path.Combine(paths))
                .Replace('/', Path.DirectorySeparatorChar);
        }

        public IVolatileToken WhenPathChanges(string path) {
            var replace = Path.Combine("~/App_Data", path).Replace(Path.DirectorySeparatorChar, '/');
            return _virtualPathMonitor.WhenPathChanges(replace);
        }

        public void CreateFile(string path, string content) {
            using(var stream = CreateFile(path)) {
                using(var tw = new StreamWriter(stream)) {
                    tw.Write(content);
                }
            }
        }

        public Stream CreateFile(string path) {
            var filePath = CombineToPhysicalPath(path);
            var folderPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            return File.Create(filePath);
        }

        public string ReadFile(string path) {
            return File.ReadAllText(CombineToPhysicalPath(path));
        }

        public Stream OpenFile(string path) {
            return File.OpenRead(CombineToPhysicalPath(path));
        }

        public void DeleteFile(string path) {
            File.Delete(CombineToPhysicalPath(path));
        }

        public bool FileExists(string path) {
            var filePath = CombineToPhysicalPath(path);
            return File.Exists(filePath);
        }

        public string Combine(params string[] paths) {
            return Path.Combine(paths).Replace(Path.DirectorySeparatorChar, '/');
        }

        public IEnumerable<string> ListFiles(string path) {
            var directoryPath = CombineToPhysicalPath(path);
            if (!Directory.Exists(directoryPath))
                return Enumerable.Empty<string>();

            var files = Directory.GetFiles(directoryPath);

            return files.Select(file => {
                                    var fileName = Path.GetFileName(file);
                                    return Combine(path, fileName);
                                });
        }

        public IEnumerable<string> ListDirectories(string path) {
            var directoryPath = CombineToPhysicalPath(path);
            if (!Directory.Exists(directoryPath))
                return Enumerable.Empty<string>();

            var files = Directory.GetDirectories(directoryPath);

            return files.Select(file => {
                                    var fileName = Path.GetFileName(file);
                                    return Combine(path, fileName);
                                });
        }

        public string CreateDirectory(string path) {
            var directory = CombineToPhysicalPath(path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return directory;
        }

        public void SetBasePath(string basePath) {
            _basePath = basePath;
        }

        public string MapPath(string path) {
            return CombineToPhysicalPath(path);
        }
    }
}