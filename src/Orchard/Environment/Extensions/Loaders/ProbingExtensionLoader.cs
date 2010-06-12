using System.IO;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;
using Orchard.FileSystems.VirtualPath;

namespace Orchard.Environment.Extensions.Loaders {
    /// <summary>
    /// Load an extension using the "Assembly.Load" method if the
    /// file can be found in the "App_Data/Dependencies" folder.
    /// </summary>
    public class ProbingExtensionLoader : IExtensionLoader {
        private readonly IDependenciesFolder _dependenciesFolder;

        public ProbingExtensionLoader(IDependenciesFolder dependenciesFolder) {
            _dependenciesFolder = dependenciesFolder;
        }

        public int Order { get { return 40; } }

        public ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            if (!_dependenciesFolder.HasPrecompiledAssembly(descriptor.Name))
                return null;

            var desc = _dependenciesFolder.GetDescriptor(descriptor.Name);
            if (desc == null)
                return null;

            return new ExtensionProbeEntry {
                    Descriptor = descriptor,
                    LastModificationTimeUtc = File.GetLastWriteTimeUtc(desc.FileName),
                    Loader = this,
                    VirtualPath = desc.VirtualPath
                };
        }

        public ExtensionEntry Load(ExtensionProbeEntry entry) {
            if (entry.Loader == this) {
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
                // 1) The webforms views which have been compiled with "Assembly Name=""" directive
                // 2) The modules which contains features which depend on us
                // 3) The binary from the App_Data directory
                //TODO
                _dependenciesFolder.Remove(entry.Descriptor.Name, this.GetType().FullName);
                return null;
            }
        }
    }
}