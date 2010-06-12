using System.IO;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;
using Orchard.FileSystems.VirtualPath;

namespace Orchard.Environment.Extensions.Loaders {
    /// <summary>
    /// Load an extension by looking into the "bin" subdirectory of an
    /// extension directory.
    /// </summary>
    public class PrecompiledExtensionLoader : IExtensionLoader {
        private readonly IDependenciesFolder _dependenciesFolder;
        private readonly IVirtualPathProvider _virtualPathProvider;

        public PrecompiledExtensionLoader(IDependenciesFolder dependenciesFolder, IVirtualPathProvider virtualPathProvider) {
            _dependenciesFolder = dependenciesFolder;
            _virtualPathProvider = virtualPathProvider;
        }

        public int Order { get { return 30; } }

        public ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            var extensionPath = _virtualPathProvider.Combine(descriptor.Location, descriptor.Name, "bin",
                                                              descriptor.Name + ".dll");
            if (!_virtualPathProvider.FileExists(extensionPath))
                return null;

            return new ExtensionProbeEntry {
                Descriptor = descriptor,
                LastModificationTimeUtc = File.GetLastWriteTimeUtc(_virtualPathProvider.MapPath(extensionPath)),
                Loader = this,
                VirtualPath = extensionPath
            };
        }

        public ExtensionEntry Load(ExtensionProbeEntry entry) {
            if (entry.Loader == this) {
                _dependenciesFolder.StorePrecompiledAssembly(entry.Descriptor.Name, entry.VirtualPath, this.GetType().FullName);

                var assembly = _dependenciesFolder.LoadAssembly(entry.Descriptor.Name);
                if (assembly == null)
                    return null;

                return new ExtensionEntry {
                    Descriptor = entry.Descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.GetExportedTypes()
                };
            }
            else {
                // If the extension is not loaded by us, there is some cached state we need to invalidate
                // 1) The webforms views which have been compiled with ".csproj" assembly source
                // 2) The modules which contains features which depend on us
                //TODO
                _dependenciesFolder.Remove(entry.Descriptor.Name, this.GetType().FullName);
                return null;
            }
        }
    }
}