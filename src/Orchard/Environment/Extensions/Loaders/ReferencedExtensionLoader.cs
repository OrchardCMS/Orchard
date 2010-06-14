using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Hosting;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;
using Orchard.FileSystems.VirtualPath;

namespace Orchard.Environment.Extensions.Loaders {
    /// <summary>
    /// Load an extension by looking through the BuildManager referenced assemblies
    /// </summary>
    public class ReferencedExtensionLoader : ExtensionLoaderBase {
        private readonly IDependenciesFolder _dependenciesFolder;
        private readonly IVirtualPathProvider _virtualPathProvider;

        public ReferencedExtensionLoader(IDependenciesFolder dependenciesFolder, IVirtualPathProvider virtualPathProvider) {
            _dependenciesFolder = dependenciesFolder;
            _virtualPathProvider = virtualPathProvider;
        }

        public override int Order { get { return 20; } }

        public override void ExtensionDeactivated(ExtensionLoadingContext ctx, bool isNewExtension, ExtensionDescriptor extension) {
            var assemblyPath = _virtualPathProvider.Combine("~/bin", extension.Name + ".dll");
            if (_virtualPathProvider.FileExists(assemblyPath)) {
                ctx.FilesToDelete.Add(_virtualPathProvider.MapPath(assemblyPath));
            }
        }

        public override ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
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
                VirtualPath = _virtualPathProvider.Combine("~/bin", descriptor.Name + ".dll")
            };
        }

        public override ExtensionEntry Load(ExtensionDescriptor descriptor) {
            if (HostingEnvironment.IsHosted == false)
                return null;

            var dependency = _dependenciesFolder.GetDescriptor(descriptor.Name);
            if (dependency != null && dependency.LoaderName == this.Name) {

                var assembly = BuildManager.GetReferencedAssemblies()
                    .OfType<Assembly>()
                    .FirstOrDefault(x => x.GetName().Name == descriptor.Name);

                return new ExtensionEntry {
                    Descriptor = descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.GetExportedTypes()
                };
            }
            return null;
        }
    }
}
