using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;
using Orchard.Environment.Extensions.Loaders;
using Orchard.FileSystems.VirtualPath;

namespace Orchard.FileSystems.Dependencies {
    public class WebFormsExtensionsVirtualPathProvider : VirtualPathProvider, ICustomVirtualPathProvider {
        private readonly IDependenciesFolder _dependenciesFolder;
        private readonly IEnumerable<IExtensionLoader> _loaders;
        private readonly string[] _prefixes = { "~/Modules/", "/Modules/" };
        private readonly string[] _extensions = { ".ascx", ".aspx", ".master" };

        public WebFormsExtensionsVirtualPathProvider(IDependenciesFolder dependenciesFolder, IEnumerable<IExtensionLoader> loaders) {
            _dependenciesFolder = dependenciesFolder;
            _loaders = loaders;
        }

        public override bool DirectoryExists(string virtualDir) {
            return Previous.DirectoryExists(virtualDir);
        }

        public override bool FileExists(string virtualPath) {
            return Previous.FileExists(virtualPath);
        }

        public override VirtualFile GetFile(string virtualPath) {
            var actualFile = Previous.GetFile(virtualPath);

            return GetCustomVirtualFile(virtualPath, actualFile) ?? actualFile;
        }

        private VirtualFile GetCustomVirtualFile(string virtualPath, VirtualFile actualFile) {
            var prefix = PrefixMatch(virtualPath, _prefixes);
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