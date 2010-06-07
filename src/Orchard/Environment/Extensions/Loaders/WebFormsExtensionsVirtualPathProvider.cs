using System;
using System.Linq;
using System.Web.Hosting;
using Orchard.FileSystems.Dependencies;

namespace Orchard.Environment.Extensions.Loaders {
    public class WebFormsExtensionsVirtualPathProvider : VirtualPathProvider, ICustomVirtualPathProvider {
        private readonly IDependenciesFolder _dependenciesFolder;
        private readonly string[] _prefixes = { "~/Modules/", "/Modules/" };
        private readonly string[] _extensions = { ".ascx", ".aspx", ".master" };

        public WebFormsExtensionsVirtualPathProvider(IDependenciesFolder dependenciesFolder) {
            _dependenciesFolder = dependenciesFolder;
        }

        public override bool DirectoryExists(string virtualDir) {
            return Previous.DirectoryExists(virtualDir);
        }

        public override bool FileExists(string virtualPath) {
            return Previous.FileExists(virtualPath);
        }

        public override VirtualFile GetFile(string virtualPath) {
            var actualFile = Previous.GetFile(virtualPath);

            var prefix = PrefixMatch(virtualPath, _prefixes);
            if (prefix == null)
                return actualFile;

            var extension = ExtensionMatch(virtualPath, _extensions);
            if (extension == null)
                return actualFile;

            var moduleName = ModuleMatch(virtualPath, prefix);
            if (moduleName == null)
                return actualFile;

            // It looks like we have a module name. Is this one of this modules
            // with its assembly stored in the "App_Data/Dependencies" folder?
            var dependencyDescriptor = _dependenciesFolder.GetDescriptor(moduleName);
            if (dependencyDescriptor == null)
                return actualFile;

            // Yes: we need to wrap the VirtualFile to add the <%@ Assembly Name=".."%> directive
            // in the content.
            return new WebFormsExtensionsVirtualFile(virtualPath, dependencyDescriptor, actualFile);
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