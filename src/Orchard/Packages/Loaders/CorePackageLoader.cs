using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Orchard.Packages.Loaders {
    public class CorePackageLoader : IPackageLoader {
        public int Order { get { return 1; } }

        public PackageEntry Load(PackageDescriptor descriptor) {
            if (descriptor.Location == "~/Core") {

                var assembly = Assembly.Load("Orchard.Core");
                return new PackageEntry {
                    Descriptor = descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.GetExportedTypes().Where(x => IsTypeFromPackage(x, descriptor))
                };
            }
            return null;
        }

        private static bool IsTypeFromPackage(Type type, PackageDescriptor descriptor) {
            return (type.Namespace + ".").StartsWith("Orchard.Core." + descriptor.Name + ".");
        }
    }
}
