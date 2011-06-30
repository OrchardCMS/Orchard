using System;
using System.IO;
using System.Reflection;
using Orchard.FileSystems.AppData;
using Orchard.Logging;

namespace Orchard.Environment.Compilation.Dependencies {
    public class DefaultAssemblyProbingFolder : IAssemblyProbingFolder {
        private const string BasePath = "Dependencies";
        private readonly IAppDataFolder _appDataFolder;
        private readonly IAssemblyLoader _assemblyLoader;

        public DefaultAssemblyProbingFolder(IAppDataFolder appDataFolder, IAssemblyLoader assemblyLoader) {
            _appDataFolder = appDataFolder;
            _assemblyLoader = assemblyLoader;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool AssemblyExists(string moduleName) {
            var path = PrecompiledAssemblyPath(moduleName);
            return File.Exists(_appDataFolder.MapPath(path));
        }

        public DateTime GetAssemblyDateTimeUtc(string moduleName) {
            var path = PrecompiledAssemblyPath(moduleName);
            string mappedPath = _appDataFolder.MapPath(path);
            if (!File.Exists(mappedPath)) {
                throw new FileNotFoundException();
            }

            return File.GetLastWriteTimeUtc(mappedPath);
        }

        public string GetAssemblyVirtualPath(string moduleName) {
            var path = PrecompiledAssemblyPath(moduleName);
            if (!File.Exists(_appDataFolder.MapPath(path)))
                return null;

            return _appDataFolder.GetVirtualPath(path);
        }

        public Assembly LoadAssembly(string moduleName) {
            var path = PrecompiledAssemblyPath(moduleName);
            if (!File.Exists(_appDataFolder.MapPath(path)))
                return null;

            return _assemblyLoader.Load(moduleName);
        }

        public void DeleteAssembly(string moduleName) {
            var path = PrecompiledAssemblyPath(moduleName);

            string targetPath = _appDataFolder.MapPath(path);
            if (File.Exists(targetPath)) {
                Logger.Information("Deleting assembly for module \"{0}\" from probing directory", moduleName);
                File.Delete(targetPath);
            }
        }

        public void StoreAssembly(string moduleName, string fileName) {
            var path = PrecompiledAssemblyPath(moduleName);

            Logger.Information("Storing assembly file \"{0}\" for module \"{1}\"", fileName, moduleName);
            string targetPath = _appDataFolder.MapPath(path);
            if (File.Exists(targetPath)) {
                File.Delete(targetPath);
            }

            File.Copy(fileName, targetPath);
        }

        private string PrecompiledAssemblyPath(string moduleName) {
            return _appDataFolder.Combine(BasePath, moduleName + ".dll");
        }
    }
}