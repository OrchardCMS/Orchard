using System;
using System.IO;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;
using Orchard.FileSystems.VirtualPath;

namespace Orchard.Environment.Extensions.Loaders {
    public class DynamicExtensionLoader : IExtensionLoader {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IBuildManager _buildManager;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IDependenciesFolder _dependenciesFolder;

        public DynamicExtensionLoader(IHostEnvironment hostEnvironment, IBuildManager buildManager, IVirtualPathProvider virtualPathProvider, IDependenciesFolder dependenciesFolder) {
            _hostEnvironment = hostEnvironment;
            _buildManager = buildManager;
            _virtualPathProvider = virtualPathProvider;
            _dependenciesFolder = dependenciesFolder;
        }

        public int Order { get { return 100; } }

        public ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            string projectPath = _virtualPathProvider.Combine(descriptor.Location, descriptor.Name,
                                                              descriptor.Name + ".csproj");
            if (!_virtualPathProvider.FileExists(projectPath)) {
                return null;
            }

            return new ExtensionProbeEntry {
                Descriptor = descriptor,
                LastModificationTimeUtc = File.GetLastWriteTimeUtc(_virtualPathProvider.MapPath(projectPath)),
                Loader = this,
                VirtualPath = projectPath
            };
        }

        public ExtensionEntry Load(ExtensionProbeEntry entry) {
            if (entry.Loader == this) {
                var assembly = _buildManager.GetCompiledAssembly(entry.VirtualPath);

                _dependenciesFolder.StoreBuildProviderAssembly(entry.Descriptor.Name, entry.VirtualPath, assembly);

                return new ExtensionEntry {
                    Descriptor = entry.Descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.GetExportedTypes(),
                };
            }
            return null;
        }
    }
}