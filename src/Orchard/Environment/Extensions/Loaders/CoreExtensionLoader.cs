using System;
using System.Linq;
using System.Reflection;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;
using Orchard.Logging;

namespace Orchard.Environment.Extensions.Loaders {
    /// <summary>
    /// Load an extension by looking into specific namespaces of the "Orchard.Core" assembly
    /// </summary>
    public class CoreExtensionLoader : ExtensionLoaderBase {
        private readonly string _coreAssemblyName = "Orchard.Core";
        private readonly IAssemblyLoader _assemblyLoader;

        public CoreExtensionLoader(IDependenciesFolder dependenciesFolder, IAssemblyLoader assemblyLoader)
            : base(dependenciesFolder) {
            _assemblyLoader = assemblyLoader;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public override int Order { get { return 10; } }

        public override ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            if (descriptor.Location == "~/Core") {
                return new ExtensionProbeEntry {
                    Descriptor = descriptor,
                    LastWriteTimeUtc = DateTime.MinValue,
                    Loader = this,
                    VirtualPath = "~/Core/" + descriptor.Name
                };
            }
            return null;
        }

        protected override ExtensionEntry LoadWorker(ExtensionDescriptor descriptor) {
            //Logger.Information("Loading extension \"{0}\"", descriptor.Name);

            var assembly = _assemblyLoader.Load(_coreAssemblyName);
            if (assembly == null) {
                Logger.Error("Core modules cannot be activated because assembly '{0}' could not be loaded", _coreAssemblyName);
                return null;
            }

            return new ExtensionEntry {
                Descriptor = descriptor,
                Assembly = assembly,
                ExportedTypes = assembly.GetExportedTypes().Where(x => IsTypeFromModule(x, descriptor))
            };
        }

        private bool IsTypeFromModule(Type type, ExtensionDescriptor descriptor) {
            return (type.Namespace + ".").StartsWith(_coreAssemblyName + "." + descriptor.Name + ".");
        }
    }
}