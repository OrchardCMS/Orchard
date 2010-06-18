using System;
using System.Linq;
using System.Reflection;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;

namespace Orchard.Environment.Extensions.Loaders {
    public class AreaExtensionLoader : ExtensionLoaderBase {
        private readonly IDependenciesFolder _dependenciesFolder;

        public AreaExtensionLoader(IDependenciesFolder dependenciesFolder) {
            _dependenciesFolder = dependenciesFolder;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public override int Order { get { return 50; } }

        public override ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            if (descriptor.Location == "~/Areas") {
                return new ExtensionProbeEntry {
                    Descriptor = descriptor,
                    Loader = this,
                    LastModificationTimeUtc = DateTime.MinValue,
                    VirtualPath = "~/Areas/" + descriptor.Name,
                };
            }
            return null;
        }

        public override ExtensionEntry Load(ExtensionDescriptor descriptor) {
            var dependency = _dependenciesFolder.GetDescriptor(descriptor.Name);
            if (dependency != null && dependency.LoaderName == this.Name) {
                Logger.Information("Loading extension \"{0}\"", dependency.Name);

                var assembly = Assembly.Load("Orchard.Web");

                return new ExtensionEntry {
                    Descriptor = descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.GetExportedTypes().Where(x => IsTypeFromModule(x, descriptor))
                };
            }
            return null;
        }

        private static bool IsTypeFromModule(Type type, ExtensionDescriptor descriptor) {
            return (type.Namespace + ".").StartsWith("Orchard.Web.Areas." + descriptor.Name + ".");
        }
    }
}