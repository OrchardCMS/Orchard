using System;
using System.Linq;
using System.Reflection;

namespace Orchard.Extensions.Loaders {
    public class CoreExtensionLoader : IExtensionLoader {
        public int Order { get { return 1; } }

        public ExtensionEntry Load(ExtensionDescriptor descriptor) {
            if (descriptor.Location == "~/Core") {

                var assembly = Assembly.Load("Orchard.Core");
                return new ExtensionEntry {
                    Descriptor = descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.GetExportedTypes().Where(x => IsTypeFromPackage(x, descriptor))
                };
            }
            return null;
        }

        private static bool IsTypeFromPackage(Type type, ExtensionDescriptor descriptor) {
            return (type.Namespace + ".").StartsWith("Orchard.Core." + descriptor.Name + ".");
        }
    }
}
