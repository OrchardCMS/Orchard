using System;
using System.Linq;
using System.Reflection;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions.Loaders {
    /// <summary>
    /// Load an extension by looking into specific namespaces of the "Orchard.Core" assembly
    /// </summary>
    public class CoreExtensionLoader : IExtensionLoader {
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
                return new ExtensionEntry {
                    Descriptor = entry.Descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.GetExportedTypes().Where(x => IsTypeFromModule(x, entry.Descriptor))
                };
            }
            return null;
        }

        private static bool IsTypeFromModule(Type type, ExtensionDescriptor descriptor) {
            return (type.Namespace + ".").StartsWith("Orchard.Core." + descriptor.Name + ".");
        }
    }
}