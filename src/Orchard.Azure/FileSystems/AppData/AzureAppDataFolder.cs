using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.AppData;

namespace Orchard.Azure.FileSystems.AppData {
    public class AzureAppDataFolder : IAppDataFolder {

        private readonly AzureFileSystem _fs;

        public AzureAppDataFolder() {
            _fs = new AzureFileSystem("appdata", null, true);
        }

        public void CreateFile(string path, string content) {
            using ( new HttpContextWeaver() ) {
                if (_fs.FileExists(path)) {
                    DeleteFile(path);
                }

                using (var stream = _fs.CreateFile(path).OpenWrite()) {
                    using (var writer = new StreamWriter(stream)) {
                        writer.Write(content);
                    }
                }
            }
        }

        public string ReadFile(string path) {
            using ( new HttpContextWeaver() ) {
                using (var stream = _fs.GetFile(path).OpenRead()) {
                    using (var reader = new StreamReader(stream)) {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        public void DeleteFile(string path) {
            _fs.DeleteFile(path);
        }

        public bool FileExists(string path) {
            return _fs.FileExists(path);
        }

        public IEnumerable<string> ListFiles(string path) {
            return _fs.ListFiles(path).Select(sf => sf.GetPath());
        }

        public IEnumerable<string> ListDirectories(string path) {
            return _fs.ListFolders(path).Select(sf => sf.GetName());
        }

        public string CreateDirectory(string path) {
            _fs.CreateFolder(path);
            return Path.GetDirectoryName(path);
        }

        public void SetBasePath(string basePath) {
            throw new NotImplementedException();
        }

        public string MapPath(string path) {
            throw new NotImplementedException();
        }
    }
}