using System;
using System.Reflection;
using Orchard.FileSystems.AppData;

namespace Orchard.FileSystems.Dependencies {
    public class DefaultAssemblyProbingFolder : IAssemblyProbingFolder {
        private const string BasePath = "Dependencies";
        private readonly IAppDataFolder _appDataFolder;

        public DefaultAssemblyProbingFolder(IAppDataFolder appDataFolder) {
            _appDataFolder = appDataFolder;
        }

        public bool AssemblyExists(string moduleName) {
            var path = PrecompiledAssemblyPath(moduleName);
            return _appDataFolder.FileExists(path);
        }

        public DateTime GetAssemblyDateTimeUtc(string moduleName) {
            var path = PrecompiledAssemblyPath(moduleName);
            return _appDataFolder.GetFileLastWriteTimeUtc(path);
        }

        public Assembly LoadAssembly(string moduleName) {
            var path = PrecompiledAssemblyPath(moduleName);
            if (!_appDataFolder.FileExists(path))
                return null;

            return Assembly.Load(moduleName);
        }

        public void DeleteAssembly(string moduleName) {
            var path = PrecompiledAssemblyPath(moduleName);
            _appDataFolder.DeleteFile(path);
        }

        public void StoreAssembly(string moduleName, string fileName) {
            var path = PrecompiledAssemblyPath(moduleName);
            _appDataFolder.StoreFile(fileName, path);
        }

        private string PrecompiledAssemblyPath(string moduleName) {
            return _appDataFolder.Combine(BasePath, moduleName + ".dll");
        }
    }
}