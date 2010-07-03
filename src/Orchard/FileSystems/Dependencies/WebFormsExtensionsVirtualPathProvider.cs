using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;
using Orchard.Environment.Extensions.Loaders;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;

namespace Orchard.FileSystems.Dependencies {
    public class WebFormVirtualPathProvider : VirtualPathProvider, ICustomVirtualPathProvider {
        private readonly IDependenciesFolder _dependenciesFolder;
        private readonly IEnumerable<IExtensionLoader> _loaders;
        private readonly string[] _modulesPrefixes = { "~/Modules/", "/Modules/" };
        private readonly string[] _themesPrefixes = { "~/Themes/", "/Themes/" };
        private readonly string[] _extensions = { ".ascx", ".aspx", ".master" };

        public WebFormVirtualPathProvider(IDependenciesFolder dependenciesFolder, IEnumerable<IExtensionLoader> loaders) {
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
            Logger.Information("GetFileHash(\"{0}\"): {1}", virtualPath, result);
            return result;
        }

        private string GetFileHashWorker(string virtualPath, IEnumerable virtualPathDependencies) {
            // We override the "GetFileHash()" behavior to take into account the dependencies folder
            // state. This ensures that if any dependency changes, ASP.NET will recompile views that
            // have been customized to include custom assembly directives.
            var file = GetModuleVirtualOverride(virtualPath) ?? GetThemeVirtualOverride(virtualPath);
            if (file == null) {
                return base.GetFileHash(virtualPath, virtualPathDependencies);
            }

            var dependencies =
                virtualPathDependencies
                    .OfType<string>()
                    .Concat(file.Loaders.SelectMany(dl => dl.Loader.GetWebFormVirtualDependencies(dl.Descriptor)));

            if (Logger.IsEnabled(LogLevel.Debug)) {
                Logger.Debug("GetFileHash(\"{0}\") - virtual path dependencies:", virtualPath);
                foreach(var dependency in dependencies) {
                    Logger.Debug("  Dependency: \"{0}\"", dependency);
                }
            }
            return base.GetFileHash(virtualPath, dependencies);
        }

        public override VirtualFile GetFile(string virtualPath) {
            //Logger.Debug("GetFile(\"{0}\")", virtualPath);
            var actualFile = Previous.GetFile(virtualPath);

            return GetModuleCustomVirtualFile(virtualPath, actualFile) ??
                   GetThemeCustomVirtualFile(virtualPath, actualFile) ??
                   actualFile;
        }

        private VirtualFile GetModuleCustomVirtualFile(string virtualPath, VirtualFile actualFile) {
            var file = GetModuleVirtualOverride(virtualPath);
            if (file == null)
                return null;

            if (Logger.IsEnabled(LogLevel.Debug)) {
                Logger.Debug("Virtual file from module \"{0}\" served with specific assembly directive:", file.ModuleName);
                Logger.Debug("  Virtual path:       {0}", virtualPath);
                Logger.Debug("  Assembly directive: {0}", file.Directive);
            }

            return new WebFormVirtualFile(virtualPath, actualFile, file.Directive);
        }

        private VirtualFile GetThemeCustomVirtualFile(string virtualPath, VirtualFile actualFile) {
            var file = GetThemeVirtualOverride(virtualPath);
            if (file == null)
                return null;

            if (Logger.IsEnabled(LogLevel.Debug)) {
                Logger.Debug("Virtual file from theme served with specific assembly directive:");
                Logger.Debug("  Virtual path:       {0}", virtualPath);
                Logger.Debug("  Assembly directive: {0}", file.Directive);
            }

            return new WebFormVirtualFile(virtualPath, actualFile, file.Directive);
        }

        private VirtualFileOverride GetModuleVirtualOverride(string virtualPath) {
            var prefix = PrefixMatch(virtualPath, _modulesPrefixes);
            if (prefix == null)
                return null;

            var extension = ExtensionMatch(virtualPath, _extensions);
            if (extension == null)
                return null;

            var moduleName = ModuleMatch(virtualPath, prefix);
            if (moduleName == null)
                return null;

            var dependencyDescriptor = _dependenciesFolder.GetDescriptor(moduleName);
            if (dependencyDescriptor == null)
                return null;

            var loader = _loaders.Where(l => l.Name == dependencyDescriptor.LoaderName).FirstOrDefault();
            if (loader == null)
                return null;

            var directive = loader.GetWebFormAssemblyDirective(dependencyDescriptor);
            if (string.IsNullOrEmpty(directive))
                return null;

            return new VirtualFileOverride {
                ModuleName = moduleName,
                Directive = directive,
                Loaders = new[] { new DependencyLoader { Loader = loader, Descriptor = dependencyDescriptor } }
            };
        }

        private VirtualFileOverride GetThemeVirtualOverride(string virtualPath) {
            var prefix = PrefixMatch(virtualPath, _themesPrefixes);
            if (prefix == null)
                return null;

            var extension = ExtensionMatch(virtualPath, _extensions);
            if (extension == null)
                return null;

            var loaders = _loaders
                .SelectMany(loader => _dependenciesFolder
                                          .LoadDescriptors()
                                          .Where(d => d.LoaderName == loader.Name),
                            (loader, desr) => new DependencyLoader { Loader = loader, Descriptor = desr });

            var directive = loaders
                .Aggregate("", (s, dl) => s + dl.Loader.GetWebFormAssemblyDirective(dl.Descriptor));

            if (string.IsNullOrEmpty(directive))
                return null;

            return new VirtualFileOverride {
                ModuleName = "",
                Directive = directive,
                Loaders = loaders
            };
        }

        private string ModuleMatch(string virtualPath, string prefix) {
            var index = virtualPath.IndexOf('/', prefix.Length, virtualPath.Length - prefix.Length);
            if (index < 0)
                return null;

            var moduleName = virtualPath.Substring(prefix.Length, index - prefix.Length);
            return (string.IsNullOrEmpty(moduleName) ? null : moduleName);
        }

        private string ExtensionMatch(string virtualPath, params string[] extensions) {
            return extensions
                .FirstOrDefault(e => virtualPath.EndsWith(e, StringComparison.OrdinalIgnoreCase));
        }

        private string PrefixMatch(string virtualPath, params string[] prefixes) {
            return prefixes
                .FirstOrDefault(p => virtualPath.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }

        VirtualPathProvider ICustomVirtualPathProvider.Instance {
            get { return this; }
        }

        private class VirtualFileOverride {
            public string ModuleName { get; set; }
            public string Directive { get; set; }
            public IEnumerable<DependencyLoader> Loaders { get; set; }
        }

        private class DependencyLoader {
            public IExtensionLoader Loader { get; set; }
            public DependencyDescriptor Descriptor { get; set; }
        }
    }
}