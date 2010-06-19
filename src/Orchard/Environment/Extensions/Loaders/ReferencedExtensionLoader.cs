using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Hosting;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;

namespace Orchard.Environment.Extensions.Loaders {
    /// <summary>
    /// Load an extension by looking through the BuildManager referenced assemblies
    /// </summary>
    public class ReferencedExtensionLoader : ExtensionLoaderBase {
        private readonly IVirtualPathProvider _virtualPathProvider;

        public ReferencedExtensionLoader(IDependenciesFolder dependenciesFolder, IVirtualPathProvider virtualPathProvider)
            : base(dependenciesFolder) {

            _virtualPathProvider = virtualPathProvider;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public override int Order { get { return 20; } }

        public override void ExtensionDeactivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
            DeleteAssembly(ctx, extension.Name);
        }

        public override void ExtensionRemoved(ExtensionLoadingContext ctx, DependencyDescriptor dependency) {
            DeleteAssembly(ctx, dependency.Name);
        }

        private void DeleteAssembly(ExtensionLoadingContext ctx, string moduleName) {
            var assemblyPath = _virtualPathProvider.Combine("~/bin", moduleName + ".dll");
            if (_virtualPathProvider.FileExists(assemblyPath)) {
                ctx.DeleteActions.Add(
                    () => {
                        Logger.Information("ExtensionRemoved: Deleting assembly \"{0}\" from bin directory (AppDomain will restart)", moduleName);
                        File.Delete(_virtualPathProvider.MapPath(assemblyPath));
                    });
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

        protected override ExtensionEntry LoadWorker(ExtensionDescriptor descriptor) {
            if (HostingEnvironment.IsHosted == false)
                return null;

            var assembly = BuildManager.GetReferencedAssemblies()
                .OfType<Assembly>()
                .FirstOrDefault(x => x.GetName().Name == descriptor.Name);

            if (assembly == null)
                return null;

            //Logger.Information("Loading extension \"{0}\"", descriptor.Name);

            return new ExtensionEntry {
                Descriptor = descriptor,
                Assembly = assembly,
                ExportedTypes = assembly.GetExportedTypes()
            };
        }
    }
}
