using System;
using System.IO;
using System.Reflection;
using Orchard.FileSystems.AppData;

namespace Orchard.FileSystems.Dependencies {
    public class DefaultAssemblyProbingFolder : IAssemblyProbingFolder {
        private const string BasePath = "Dependencies";
        private readonly IAppDataFolder _appDataFolder;

        public DefaultAssemblyProbingFolder(IAppDataFolder appDataFolder) {
            _appDataFolder = appDataFolder;
        }

        public bool HasAssembly(string moduleName) {
            var path = PrecompiledAssemblyPath(moduleName);
            return _appDataFolder.FileExists(path);
        }

        public DateTime GetAssemblyDateTimeUtc(string moduleName) {
            var path = PrecompiledAssemblyPath(moduleName);
            return File.GetLastWriteTimeUtc(_appDataFolder.MapPath(path));
        }

        public Assembly LoadAssembly(string moduleName) {
            var path = PrecompiledAssemblyPath(moduleName);
            if (!_appDataFolder.FileExists(path))
                return null;

            return Assembly.Load(moduleName);
        }

        public string GetAssemblyPhysicalFileName(string moduleName) {
            return _appDataFolder.MapPath(PrecompiledAssemblyPath(moduleName));
        }

        private string PrecompiledAssemblyPath(string moduleName) {
            return _appDataFolder.Combine(BasePath, moduleName + ".dll");
        }
    }
}