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
        private readonly IDependenciesFolder _folder;
        private readonly IVirtualPathProvider _virtualPathProvider;

        public ProbingExtensionLoader(IDependenciesFolder folder, IVirtualPathProvider virtualPathProvider) {
            _folder = folder;
        }

        public int Order { get { return 40; } }

        public ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            var desc = _folder.GetDescriptor(descriptor.Name);
            if (desc != null) {
                return new ExtensionProbeEntry {
                    Descriptor = descriptor,
                    LastModificationTimeUtc = File.GetLastWriteTimeUtc(desc.FileName),
                    Loader = this,
                    VirtualPath = desc.VirtualPath
                };
            }

            return null;
        }

        public ExtensionEntry Load(ExtensionProbeEntry entry) {
            if (entry.Loader == this) {
                var assembly = _folder.LoadAssembly(entry.Descriptor.Name);
                if (assembly == null)
                    return null;

                return new ExtensionEntry {
                    Descriptor = entry.Descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.GetExportedTypes()
                };
            }
            return null;
        }
    }
}