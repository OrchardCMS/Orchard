using System;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;

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

        public ExtensionEntry Load(ExtensionDescriptor descriptor) {
            string projectPath = _virtualPathProvider.Combine(descriptor.Location, descriptor.Name,
                                                              descriptor.Name + ".csproj");
            if (!_virtualPathProvider.FileExists(projectPath)) {
                return null;
            }

            var assembly = _buildManager.GetCompiledAssembly(projectPath);

            if (_hostEnvironment.IsFullTrust) {
                _dependenciesFolder.StoreAssemblyFile(descriptor.Name, assembly.Location);
            }

            return new ExtensionEntry {
                Descriptor = descriptor,
                Assembly = assembly,
                ExportedTypes = assembly.GetExportedTypes(),
            };
        }
    }
}