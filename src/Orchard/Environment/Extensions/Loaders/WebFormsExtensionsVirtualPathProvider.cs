using System;
using System.Linq;
using System.Web.Hosting;
using Orchard.FileSystems.Dependencies;

namespace Orchard.Environment.Extensions.Loaders {
    public class WebFormsExtensionsVirtualPathProvider : VirtualPathProvider {
        private IDependenciesFolder _dependenciesFolder;
        private const string _prefix1 = "~/Modules/";
        private const string _prefix2 = "/Modules/";

        public WebFormsExtensionsVirtualPathProvider() {
        }

        protected override void Initialize() {
            base.Initialize();
            _dependenciesFolder = new DefaultDependenciesFolder(new DefaultVirtualPathProvider());
        }

        public override bool DirectoryExists(string virtualDir) {
            return Previous.DirectoryExists(virtualDir);
        }

        public override bool FileExists(string virtualPath) {
            return Previous.FileExists(virtualPath);
        }

        public override VirtualFile GetFile(string virtualPath) {
            var actualFile = Previous.GetFile(virtualPath);

            var prefix = PrefixMatch(virtualPath);
            if (prefix == null)
                return actualFile;

            var extension = ExtensionMatch(virtualPath, ".ascx", ".aspx", ".master");
            if (extension == null)
                return actualFile;

            var moduleName = ModuleMatch(virtualPath, prefix);
            if (moduleName == null)
                return actualFile;

            // It looks like we have a module name. Is this one of this modules
            // with its assembly stored in the "App_Data/Dependencies" folder?
            var assembly = _dependenciesFolder.LoadAssembly(moduleName);
            if (assembly == null)
                return actualFile;

            // Yes: we need to wrap the VirtualFile to add the <%@ Assembly Name=".."%> directive
            // in the content.
            return new WebFormsExtensionsVirtualFile(virtualPath, assembly, actualFile);
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

        private string PrefixMatch(string virtualPath) {
            if (virtualPath.StartsWith(_prefix1))
                return _prefix1;
            if (virtualPath.StartsWith(_prefix2))
                return _prefix2;
            return null;

        }
    }
}