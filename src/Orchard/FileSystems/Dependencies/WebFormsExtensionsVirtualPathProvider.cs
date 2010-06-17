using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;
using Orchard.Environment.Extensions.Loaders;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;

namespace Orchard.FileSystems.Dependencies {
    public class WebFormsExtensionsVirtualPathProvider : VirtualPathProvider, ICustomVirtualPathProvider {
        private readonly IDependenciesFolder _dependenciesFolder;
        private readonly IEnumerable<IExtensionLoader> _loaders;
        private readonly string[] _modulesPrefixes = { "~/Modules/", "/Modules/" };
        private readonly string[] _themesPrefixes = { "~/Themes/", "/Themes/" };
        private readonly string[] _extensions = { ".ascx", ".aspx", ".master" };

        public WebFormsExtensionsVirtualPathProvider(IDependenciesFolder dependenciesFolder, IEnumerable<IExtensionLoader> loaders) {
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

        public override VirtualFile GetFile(string virtualPath) {
            var actualFile = Previous.GetFile(virtualPath);

            return 
                GetModuleCustomVirtualFile(virtualPath, actualFile) ??
                GetThemeCustomVirtualFile(virtualPath, actualFile) ??
                actualFile;
        }

        private VirtualFile GetModuleCustomVirtualFile(string virtualPath, VirtualFile actualFile) {
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

            var directive = loader.GetAssemblyDirective(dependencyDescriptor);
            if (string.IsNullOrEmpty(directive))
                return null;

            if (Logger.IsEnabled(LogLevel.Debug)) {
                Logger.Debug("Virtual file from module \"{0}\" served with specific assembly directive:", moduleName);
                Logger.Debug("  Virtual path:       {0}", virtualPath);
                Logger.Debug("  Assembly directive: {0}", directive);
            }

            return new WebFormsExtensionsVirtualFile(virtualPath, actualFile, directive);
        }

        private VirtualFile GetThemeCustomVirtualFile(string virtualPath, VirtualFile actualFile) {
            var prefix = PrefixMatch(virtualPath, _themesPrefixes);
            if (prefix == null)
                return null;

            var extension = ExtensionMatch(virtualPath, _extensions);
            if (extension == null)
                return null;

            string directive = _dependenciesFolder.LoadDescriptors().Aggregate("", (s, desc) => {
                var loader = _loaders.Where(l => l.Name == desc.LoaderName).FirstOrDefault();
                if (loader == null)
                    return s;
                else {
                    return s + loader.GetAssemblyDirective(desc);
                }});

            if (string.IsNullOrEmpty(directive))
                return null;

            if (Logger.IsEnabled(LogLevel.Debug)) {
                Logger.Debug("Virtual file from theme served with specific assembly directive:");
                Logger.Debug("  Virtual path:       {0}", virtualPath);
                Logger.Debug("  Assembly directive: {0}", directive);
            }

            return new WebFormsExtensionsVirtualFile(virtualPath, actualFile, directive);
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
    }
}