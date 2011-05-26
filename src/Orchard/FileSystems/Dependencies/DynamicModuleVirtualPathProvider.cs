using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Orchard.Environment.Extensions.Loaders;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;

namespace Orchard.FileSystems.Dependencies {
    /// <summary>
    /// The purpose of this virtual path provider is to add file dependencies to .csproj files
    /// served from the "~/Modules" or "~/Themes" directory.
    /// </summary>
    public class DynamicModuleVirtualPathProvider : VirtualPathProvider, ICustomVirtualPathProvider {
        private readonly IDependenciesFolder _dependenciesFolder;
        private readonly IEnumerable<IExtensionLoader> _loaders;
        private readonly string[] _modulesPrefixes = { "~/Modules/", "~/Themes/" };

        public DynamicModuleVirtualPathProvider(IDependenciesFolder dependenciesFolder, IEnumerable<IExtensionLoader> loaders) {
            _dependenciesFolder = dependenciesFolder;
            _loaders = loaders;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public override bool DirectoryExists(string virtualDir) {
            return Previous.DirectoryExists(virtualDir);
        }

        public override bool FileExists(string virtualPath) {
            return Previous.FileExists(virtualPath);
        }

        public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies) {
            var result = GetFileHashWorker(virtualPath, virtualPathDependencies);
            Logger.Debug("GetFileHash(\"{0}\"): {1}", virtualPath, result);
            return result;
        }

        private string GetFileHashWorker(string virtualPath, IEnumerable virtualPathDependencies) {
            var desc = GetDependencyDescriptor(virtualPath);
            if (desc != null) {

                var loader = _loaders.Where(l => l.Name == desc.LoaderName).FirstOrDefault() as DynamicExtensionLoader;
                if (loader != null) {

                    var otherDependencies = loader.GetDynamicModuleDependencies(desc, virtualPath);
                    if (otherDependencies.Any()) {

                        var allDependencies = virtualPathDependencies.OfType<string>().Concat(otherDependencies);

                        if (Logger.IsEnabled(LogLevel.Debug)) {
                            Logger.Debug("GetFileHash(\"{0}\") - virtual path dependencies:", virtualPath);
                            foreach (var dependency in allDependencies) {
                                Logger.Debug("  Dependency: \"{0}\"", dependency);
                            }
                        }

                        return base.GetFileHash(virtualPath, allDependencies);
                    }
                }
            }
            return base.GetFileHash(virtualPath, virtualPathDependencies);
        }

        public override VirtualFile GetFile(string virtualPath) {
            return Previous.GetFile(virtualPath);
        }

        private DependencyDescriptor GetDependencyDescriptor(string virtualPath) {
            var appRelativePath = VirtualPathUtility.ToAppRelative(virtualPath);
            var prefix = PrefixMatch(appRelativePath, _modulesPrefixes);
            if (prefix == null)
                return null;

            var moduleName = ModuleMatch(appRelativePath, prefix);
            if (moduleName == null)
                return null;

            return _dependenciesFolder.GetDescriptor(moduleName);
        }

        private static string ModuleMatch(string virtualPath, string prefix) {
            var index = virtualPath.IndexOf('/', prefix.Length, virtualPath.Length - prefix.Length);
            if (index < 0)
                return null;

            var moduleName = virtualPath.Substring(prefix.Length, index - prefix.Length);
            return (string.IsNullOrEmpty(moduleName) ? null : moduleName);
        }

        private static string PrefixMatch(string virtualPath, params string[] prefixes) {
            return prefixes
                .FirstOrDefault(p => virtualPath.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }

        VirtualPathProvider ICustomVirtualPathProvider.Instance {
            get { return this; }
        }
    }
}