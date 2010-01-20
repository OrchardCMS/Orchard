using System;
using System.Linq;
using System.Reflection;

namespace Orchard.Extensions.Loaders {
    public class AreaExtensionLoader : IExtensionLoader {
        public int Order { get { return 5; } }

        public ExtensionEntry Load(ExtensionDescriptor descriptor) {
            if (descriptor.Location == "~/Areas") {

                var assembly = Assembly.Load("Orchard.Web");
                return new ExtensionEntry {
                    Descriptor = descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.GetExportedTypes().Where(x => IsTypeFromPackage(x, descriptor))
                };
            }
            return null;
        }

        private static bool IsTypeFromPackage(Type type, ExtensionDescriptor descriptor) {
            return (type.Namespace + ".").StartsWith("Orchard.Web.Areas." + descriptor.Name + ".");
        }
    }
}
