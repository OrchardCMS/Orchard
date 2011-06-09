using System;
using System.Linq;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;
using Orchard.Logging;

namespace Orchard.Environment.Extensions.Loaders {
    /// <summary>
    /// Load an extension by looking into specific namespaces of the "Orchard.Core" assembly
    /// </summary>
    public class CoreExtensionLoader : ExtensionLoaderBase {
        private const string CoreAssemblyName = "Orchard.Core";
        private readonly IAssemblyLoader _assemblyLoader;

        public CoreExtensionLoader(IDependenciesFolder dependenciesFolder, IAssemblyLoader assemblyLoader)
            : base(dependenciesFolder) {
            _assemblyLoader = assemblyLoader;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public bool Disabled { get; set; }

        public override int Order { get { return 10; } }

        public override ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            if (Disabled)
                return null;

            if (descriptor.Location == "~/Core") {
                return new ExtensionProbeEntry {
                    Descriptor = descriptor,
                    Loader = this,
                    Priority = 100, // Higher priority because assemblies in ~/bin always take precedence
                    VirtualPath = "~/Core/" + descriptor.Id,
                    VirtualPathDependencies = Enumerable.Empty<string>(),
                };
            }
            return null;
        }

        protected override ExtensionEntry LoadWorker(ExtensionDescriptor descriptor) {
            if (Disabled)
                return null;

            var assembly = _assemblyLoader.Load(CoreAssemblyName);
            if (assembly == null) {
                Logger.Error("Core modules cannot be activated because assembly '{0}' could not be loaded", CoreAssemblyName);
                return null;
            }

            Logger.Information("Loaded core module \"{0}\": assembly name=\"{1}\"", descriptor.Name, assembly.FullName);

            return new ExtensionEntry {
                Descriptor = descriptor,
                Assembly = assembly,
                ExportedTypes = assembly.GetExportedTypes().Where(x => IsTypeFromModule(x, descriptor))
            };
        }

        private static bool IsTypeFromModule(Type type, ExtensionDescriptor descriptor) {
            return (type.Namespace + ".").StartsWith(CoreAssemblyName + "." + descriptor.Id + ".");
        }
    }
}