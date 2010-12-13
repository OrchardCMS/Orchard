using System;
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
        public bool Disabled { get; set; }

        public override int Order { get { return 10; } }

        public override ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            if (Disabled)
                return null;

            if (descriptor.Location == "~/Themes") {
                string projectPath = _virtualPathProvider.Combine(descriptor.Location, descriptor.Id,
                                           descriptor.Id + ".csproj");

                // ignore themes including a .csproj in this loader
                if ( _virtualPathProvider.FileExists(projectPath) ) {
                    return null;
                }

                var assemblyPath = _virtualPathProvider.Combine(descriptor.Location, descriptor.Id, "bin",
                                                descriptor.Id + ".dll");

                // ignore themes with /bin in this loader
                if ( _virtualPathProvider.FileExists(assemblyPath) )
                    return null;


                return new ExtensionProbeEntry {
                    Descriptor = descriptor,
                    LastWriteTimeUtc = DateTime.MinValue,
                    Loader = this,
                    VirtualPath = "~/Theme/" + descriptor.Id
                };
            }
            return null;
        }

        protected override ExtensionEntry LoadWorker(ExtensionDescriptor descriptor) {
            if (Disabled)
                return null;

            return new ExtensionEntry {
                Descriptor = descriptor,
                Assembly = GetType().Assembly,
                ExportedTypes = new Type[0]
            };
        }
    }
}