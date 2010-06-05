using System;
using System.Linq;
using System.Reflection;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions.Loaders {
    public class AreaExtensionLoader : IExtensionLoader {
        public int Order { get { return 50; } }

        public ExtensionEntry Load(ExtensionDescriptor descriptor) {
            if (descriptor.Location == "~/Areas") {

                var assembly = Assembly.Load("Orchard.Web");
                return new ExtensionEntry {
                    Descriptor = descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.GetExportedTypes().Where(x => IsTypeFromModule(x, descriptor))
                };
            }
            return null;
        }

        private static bool IsTypeFromModule(Type type, ExtensionDescriptor descriptor) {
            return (type.Namespace + ".").StartsWith("Orchard.Web.Areas." + descriptor.Name + ".");
        }
    }
}