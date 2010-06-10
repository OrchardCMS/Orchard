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

        public ExtensionEntry Load(ExtensionDescriptor descriptor) {
            if (descriptor.Location == "~/Core") {
                var assembly = Assembly.Load("Orchard.Core");
                return new ExtensionEntry {
                    Descriptor = descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.GetExportedTypes().Where(x => IsTypeFromModule(x, descriptor))
                };
            }
            return null;
        }

        private static bool IsTypeFromModule(Type type, ExtensionDescriptor descriptor) {
            return (type.Namespace + ".").StartsWith("Orchard.Core." + descriptor.Name + ".");
        }
    }
}