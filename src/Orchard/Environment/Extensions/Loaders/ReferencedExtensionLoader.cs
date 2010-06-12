using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Hosting;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;

namespace Orchard.Environment.Extensions.Loaders {
    /// <summary>
    /// Load an extension by looking through the BuildManager referenced assemblies
    /// </summary>
    public class ReferencedExtensionLoader : IExtensionLoader {
        private readonly IDependenciesFolder _dependenciesFolder;
        public int Order { get { return 20; } }

        public ReferencedExtensionLoader(IDependenciesFolder dependenciesFolder) {
            _dependenciesFolder = dependenciesFolder;
        }

        public ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            if (HostingEnvironment.IsHosted == false)
                return null;

            var assembly = BuildManager.GetReferencedAssemblies()
                .OfType<Assembly>()
                .FirstOrDefault(x => x.GetName().Name == descriptor.Name);

            if (assembly == null)
                return null;

            return new ExtensionProbeEntry {
                Descriptor = descriptor,
                LastModificationTimeUtc = File.GetLastWriteTimeUtc(assembly.Location),
                Loader = this,
                VirtualPath = "~/bin/" + descriptor.Name
            };
        }

        public ExtensionEntry Load(ExtensionProbeEntry entry) {
            if (HostingEnvironment.IsHosted == false)
                return null;

            if (entry.Loader == this) {

                var assembly = BuildManager.GetReferencedAssemblies()
                    .OfType<Assembly>()
                    .FirstOrDefault(x => x.GetName().Name == entry.Descriptor.Name);

                _dependenciesFolder.StoreReferencedAssembly(entry.Descriptor.Name);

                return new ExtensionEntry {
                    Descriptor = entry.Descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.GetExportedTypes()
                };
            }

            return null;
        }
    }
}
