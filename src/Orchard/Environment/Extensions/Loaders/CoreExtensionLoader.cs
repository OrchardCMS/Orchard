using System;
using System.Linq;
using System.Reflection;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;
using Orchard.FileSystems.VirtualPath;

namespace Orchard.Environment.Extensions.Loaders {
    /// <summary>
    /// Load an extension by looking into specific namespaces of the "Orchard.Core" assembly
    /// </summary>
    public class CoreExtensionLoader : IExtensionLoader {
        private readonly IDependenciesFolder _dependenciesFolder;
        private readonly IVirtualPathProvider _virtualPathProvider;

        public CoreExtensionLoader(IDependenciesFolder dependenciesFolder, IVirtualPathProvider virtualPathProvider) {
            _dependenciesFolder = dependenciesFolder;
            _virtualPathProvider = virtualPathProvider;
        }

        public int Order { get { return 10; } }

        public ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            if (descriptor.Location == "~/Core") {
                return new ExtensionProbeEntry {
                    Descriptor = descriptor,
                    LastModificationTimeUtc = DateTime.MinValue,
                    Loader = this,
                    VirtualPath = "~/Core/" + descriptor.Name
                };
            }
            return null;
        }

        public ExtensionEntry Load(ExtensionProbeEntry entry) {
            if (entry.Loader == this) {
                var assembly = Assembly.Load("Orchard.Core");

                _dependenciesFolder.Store(new DependencyDescriptor { ModuleName = entry.Descriptor.Name, LoaderName = this.GetType().FullName });

                return new ExtensionEntry {
                    Descriptor = entry.Descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.GetExportedTypes().Where(x => IsTypeFromModule(x, entry.Descriptor))
                };
            }
            else {
                // If the extension is not loaded by us, there is no cached state 
                // we need to invalidate
                _dependenciesFolder.Remove(entry.Descriptor.Name, this.GetType().FullName);
                return null;
            }
        }

        private static bool IsTypeFromModule(Type type, ExtensionDescriptor descriptor) {
            return (type.Namespace + ".").StartsWith("Orchard.Core." + descriptor.Name + ".");
        }
    }
}