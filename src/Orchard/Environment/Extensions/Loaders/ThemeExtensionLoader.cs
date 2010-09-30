using System;
using System.Linq;
using System.Reflection;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;

namespace Orchard.Environment.Extensions.Loaders {
    public class RawThemeExtensionLoader : ExtensionLoaderBase {
        private readonly IVirtualPathProvider _virtualPathProvider;

        public RawThemeExtensionLoader(IDependenciesFolder dependenciesFolder, IVirtualPathProvider virtualPathProvider)
            : base(dependenciesFolder) {
            _virtualPathProvider = virtualPathProvider;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public override int Order { get { return 10; } }

        public override ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            if ( descriptor.Location == "~/Themes") {
                string projectPath = _virtualPathProvider.Combine(descriptor.Location, descriptor.Name,
                                           descriptor.Name + ".csproj");

                // ignore themes including a .csproj in this loader
                if ( _virtualPathProvider.FileExists(projectPath) ) {
                    return null;
                }

                return new ExtensionProbeEntry {
                    Descriptor = descriptor,
                    LastWriteTimeUtc = DateTime.MinValue,
                    Loader = this,
                    VirtualPath = "~/Theme/" + descriptor.Name
                };
            }
            return null;
        }

        protected override ExtensionEntry LoadWorker(ExtensionDescriptor descriptor) {
            var assembly = Assembly.Load("Orchard.Themes");

            return new ExtensionEntry {
                Descriptor = descriptor,
                Assembly = assembly,
                ExportedTypes = new Type[0]
            };
        }
    }
}