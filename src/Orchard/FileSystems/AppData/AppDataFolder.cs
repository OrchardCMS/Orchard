using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Orchard.Caching;
using Orchard.FileSystems.VirtualPath;

namespace Orchard.FileSystems.AppData {
    public class AppDataFolder : IAppDataFolder {
        private readonly IAppDataFolderRoot _root;
        private readonly IVirtualPathMonitor _virtualPathMonitor;

        public AppDataFolder(IAppDataFolderRoot root, IVirtualPathMonitor virtualPathMonitor) {
            _root = root;
            _virtualPathMonitor = virtualPathMonitor;
        }

        public string RootFolder {
            get {
                return _root.RootFolder;
            }
        }

        public string _appDataPath {
            get {
                return _root.RootPath;
            }
        }

        /// <summary>
        /// Combine a set of virtual paths relative to "~/App_Data" into an absolute physical path
        /// starting with "_basePath".
        /// </summary>
        private string CombineToPhysicalPath(params string[] paths) {
            return Path.Combine(RootFolder, Path.Combine(paths))
                .Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Combine a set of virtual paths into a virtual path relative to "~/App_Data"
        /// </summary>
        public string Combine(params string[] paths) {
            return Path.Combine(paths).Replace(Path.DirectorySeparatorChar, '/');
        }

        public IVolatileToken WhenPathChanges(string path) {
            var replace = Combine(_appDataPath, path);
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
            return File.Exists(CombineToPhysicalPath(path));
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

        public void CreateDirectory(string path) {
            Directory.CreateDirectory(CombineToPhysicalPath(path));
        }

        public string MapPath(string path) {
            return CombineToPhysicalPath(path);
        }
    }
}