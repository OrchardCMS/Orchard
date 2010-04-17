using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Hosting;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions.Loaders {
    public class ReferencedExtensionLoader : IExtensionLoader {
        public int Order { get { return 2; } }

        public ExtensionEntry Load(ExtensionDescriptor descriptor) {
            if (HostingEnvironment.IsHosted == false)
                return null;

            var assembly = BuildManager.GetReferencedAssemblies()
                .OfType<Assembly>()
                .FirstOrDefault(x => x.GetName().Name == descriptor.Name);

            if (assembly == null)
                return null;

            return new ExtensionEntry {
                                          Descriptor = descriptor,
                                          Assembly = assembly,
                                          ExportedTypes = assembly.GetExportedTypes()
                                      };
        }
    }
}