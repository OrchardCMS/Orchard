using System;
using System.Linq;
using System.Reflection;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions.Loaders {
    public class AreaExtensionLoader : IExtensionLoader {
        public int Order { get { return 50; } }

        public ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            if (descriptor.Location == "~/Areas") {
                return new ExtensionProbeEntry {
                    Descriptor = descriptor,
                    Loader = this,
                    LastModificationTimeUtc = DateTime.MinValue,
                    VirtualPath = "~/Areas/" + descriptor.Name,
                };
            }
            return null;
        }

        public ExtensionEntry Load(ExtensionProbeEntry entry) {
            if (entry.Loader == this) {
                var assembly = Assembly.Load("Orchard.Web");
                return new ExtensionEntry {
                    Descriptor = entry.Descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.GetExportedTypes().Where(x => IsTypeFromModule(x, entry.Descriptor))
                };
            }
            return null;
        }

        private static bool IsTypeFromModule(Type type, ExtensionDescriptor descriptor) {
            return (type.Namespace + ".").StartsWith("Orchard.Web.Areas." + descriptor.Name + ".");
        }
    }
}