using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Orchard.Packages.Loaders {
    public class PrecompiledPackageLoader : IPackageLoader {
        public int Order { get { return 3; } }

        public PackageEntry Load(PackageDescriptor descriptor) {
            //var assembly = Assembly.Load(descriptor.Name);
            //return new PackageEntry {
            //    Descriptor = descriptor,
            //    Assembly = assembly,
            //    ExportedTypes = assembly.GetExportedTypes()
            //};
            return null;
        }
    }
}
