using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Compilation;
using System.Web.Hosting;

namespace Orchard.Packages.Loaders {
    public class ReferencedPackageLoader : IPackageLoader {
        public int Order { get { return 2; } }

        public PackageEntry Load(PackageDescriptor descriptor) {
            if (HostingEnvironment.IsHosted == false)
                return null;

            var assembly = BuildManager.GetReferencedAssemblies()
                .OfType<Assembly>()
                .FirstOrDefault(x=>x.GetName().Name == descriptor.Name);
            
            if (assembly == null)
                return null;
            
            return new PackageEntry {
                Descriptor = descriptor,
                Assembly = assembly,
                ExportedTypes = assembly.GetExportedTypes()
            };
        }
    }
}
