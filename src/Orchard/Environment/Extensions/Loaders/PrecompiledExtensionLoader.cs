using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;

namespace Orchard.Environment.Extensions.Loaders {
    /// <summary>
    /// Load an extension by looking into the "bin" subdirectory of an
    /// extension directory.
    /// </summary>
    public class PrecompiledExtensionLoader : IExtensionLoader {
        private readonly IDependenciesFolder _folder;
        private readonly IVirtualPathProvider _virtualPathProvider;

        public PrecompiledExtensionLoader(IDependenciesFolder folder, IVirtualPathProvider virtualPathProvider) {
            _folder = folder;
            _virtualPathProvider = virtualPathProvider;
        }

        public int Order { get { return 30; } }

        public ExtensionEntry Load(ExtensionDescriptor descriptor) {
            var extensionPath = _virtualPathProvider.Combine(descriptor.Location, descriptor.Name, "bin",
                                                              descriptor.Name + ".dll");
            if (!_virtualPathProvider.FileExists(extensionPath))
                return null;

            _folder.StorePrecompiledAssembly(descriptor.Name, extensionPath);

            var assembly = _folder.LoadAssembly(descriptor.Name);
            if (assembly == null)
                return null;

            return new ExtensionEntry {
                Descriptor = descriptor,
                Assembly = assembly,
                ExportedTypes = assembly.GetExportedTypes()
            };
        }
    }
}