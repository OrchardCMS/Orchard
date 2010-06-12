using System;
using System.IO;
using Orchard.Caching;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;
using Orchard.FileSystems.VirtualPath;

namespace Orchard.Environment.Extensions.Loaders {
    public class DynamicExtensionLoader : IExtensionLoader {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IBuildManager _buildManager;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IVirtualPathMonitor _virtualPathMonitor;
        private readonly IDependenciesFolder _dependenciesFolder;

        public DynamicExtensionLoader(IHostEnvironment hostEnvironment, 
            IBuildManager buildManager, 
            IVirtualPathProvider virtualPathProvider, 
            IVirtualPathMonitor virtualPathMonitor, 
            IDependenciesFolder dependenciesFolder) {
            _hostEnvironment = hostEnvironment;
            _buildManager = buildManager;
            _virtualPathProvider = virtualPathProvider;
            _virtualPathMonitor = virtualPathMonitor;
            _dependenciesFolder = dependenciesFolder;
        }

        public int Order { get { return 100; } }

        public void Monitor(ExtensionDescriptor descriptor, Action<IVolatileToken> monitor) {
            string projectPath = GetProjectPath(descriptor);
            if (projectPath != null)
                monitor(_virtualPathMonitor.WhenPathChanges(projectPath));
        }

        public ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            string projectPath = GetProjectPath(descriptor);
            if (projectPath == null)
                return null;

            return new ExtensionProbeEntry {
                Descriptor = descriptor,
                LastModificationTimeUtc = File.GetLastWriteTimeUtc(_virtualPathProvider.MapPath(projectPath)),
                Loader = this,
                VirtualPath = projectPath
            };
        }

        private string GetProjectPath(ExtensionDescriptor descriptor) {
            string projectPath = _virtualPathProvider.Combine(descriptor.Location, descriptor.Name,
                                                       descriptor.Name + ".csproj");

            if (!_virtualPathProvider.FileExists(projectPath)) {
                return null;
            }

            return projectPath;
        }


        public ExtensionEntry Load(ExtensionProbeEntry entry) {
            if (entry.Loader == this) {
                var assembly = _buildManager.GetCompiledAssembly(entry.VirtualPath);

                _dependenciesFolder.Store(new DependencyDescriptor {
                    ModuleName = entry.Descriptor.Name, 
                    LoaderName = this.GetType().FullName,
                    VirtualPath = entry.VirtualPath,
                    FileName = assembly.Location
                });

                return new ExtensionEntry {
                    Descriptor = entry.Descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.GetExportedTypes(),
                };
            }
            else {
                // If the extension is not loaded by us, there is some cached state we need to invalidate
                // 1) The webforms views which have been compiled with ".csproj" assembly source
                // 2) The modules which contains features which depend on us
                //TODO
                _dependenciesFolder.Remove(entry.Descriptor.Name, this.GetType().FullName);
                return null;
            }
        }
    }
}