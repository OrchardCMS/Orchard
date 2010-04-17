using System;
using System.Linq;
using System.Reflection;
using Orchard.Extensions.Models;

namespace Orchard.Extensions.Loaders {
    public class CoreExtensionLoader : IExtensionLoader {
        public int Order { get { return 1; } }

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
