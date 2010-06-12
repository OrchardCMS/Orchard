using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Hosting;
using Orchard.Caching;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;

namespace Orchard.Environment.Extensions.Loaders {
    /// <summary>
    /// Load an extension by looking through the BuildManager referenced assemblies
    /// </summary>
    public class ReferencedExtensionLoader : IExtensionLoader {
        private readonly IDependenciesFolder _dependenciesFolder;

        public ReferencedExtensionLoader(IDependenciesFolder dependenciesFolder) {
            _dependenciesFolder = dependenciesFolder;
        }

        public int Order { get { return 20; } }

        public void Monitor(ExtensionDescriptor descriptor, Action<IVolatileToken> monitor) {
            // We don't monitor assemblies loaded from the "~/bin" directory,
            // because they are monitored by the ASP.NET runtime.
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

                _dependenciesFolder.Store(new DependencyDescriptor {
                    ModuleName = entry.Descriptor.Name, 
                    LoaderName = this.GetType().FullName,
                    VirtualPath = entry.VirtualPath,
                    FileName = assembly.Location
                });


                return new ExtensionEntry {
                    Descriptor = entry.Descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.GetExportedTypes()
                };
            }
            else {
                // If the extension is not loaded by us, there is no cached state 
                // we need to invalidate
                _dependenciesFolder.Remove(entry.Descriptor.Name, this.GetType().FullName);
                return null;
            }
        }
    }
}
