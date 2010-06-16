using System;
using System.IO;
using Orchard.Caching;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;

namespace Orchard.Environment.Extensions.Loaders {
    /// <summary>
    /// Load an extension by looking into the "bin" subdirectory of an
    /// extension directory.
    /// </summary>
    public class PrecompiledExtensionLoader : ExtensionLoaderBase {
        private readonly IDependenciesFolder _dependenciesFolder;
        private readonly IAssemblyProbingFolder _assemblyProbingFolder;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IVirtualPathMonitor _virtualPathMonitor;

        public PrecompiledExtensionLoader(IDependenciesFolder dependenciesFolder, IAssemblyProbingFolder assemblyProbingFolder, IVirtualPathProvider virtualPathProvider, IVirtualPathMonitor virtualPathMonitor) {
            _dependenciesFolder = dependenciesFolder;
            _assemblyProbingFolder = assemblyProbingFolder;
            _virtualPathProvider = virtualPathProvider;
            _virtualPathMonitor = virtualPathMonitor;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public override int Order { get { return 30; } }

        public override string GetAssemblyDirective(DependencyDescriptor dependency) {
            return string.Format("<%@ Assembly Name=\"{0}\"%>", dependency.Name);
        }

        public override void ExtensionRemoved(ExtensionLoadingContext ctx, DependencyDescriptor dependency) {
            var assemblyFileName = _assemblyProbingFolder.GetAssemblyPhysicalFileName(dependency.Name);
            if (File.Exists(assemblyFileName)) {
                ctx.FilesToDelete.Add(assemblyFileName);

                // We need to restart the appDomain if the assembly is loaded
                if (IsAssemblyLoaded(dependency.Name)) {
                    Logger.Information("Extension removed: Setting AppDomain for restart because assembly {0} is loaded", dependency.Name);
                    ctx.RestartAppDomain = true;
                }
            }
        }

        public override void ExtensionActivated(ExtensionLoadingContext ctx, bool isNewExtension, ExtensionDescriptor extension) {
            string sourceFileName = _virtualPathProvider.MapPath(GetAssemblyPath(extension));
            string destinationFileName = _assemblyProbingFolder.GetAssemblyPhysicalFileName(extension.Name);
            if (FileIsNewer(sourceFileName, destinationFileName)) {
                ctx.FilesToCopy.Add(sourceFileName, destinationFileName);
                // We need to restart the appDomain if the assembly is loaded
                if (IsAssemblyLoaded(extension.Name)) {
                    Logger.Information("Extension activated: Setting AppDomain for restart because assembly {0} is loaded", extension.Name);
                    ctx.RestartAppDomain = true;
                }
            }
        }

        public override void ExtensionDeactivated(ExtensionLoadingContext ctx, bool isNewExtension, ExtensionDescriptor extension) {
            var assemblyFileName = _assemblyProbingFolder.GetAssemblyPhysicalFileName(extension.Name);
            if (File.Exists(assemblyFileName)) {
                ctx.FilesToDelete.Add(assemblyFileName);
                // We need to restart the appDomain if the assembly is loaded
                if (IsAssemblyLoaded(extension.Name)) {
                    Logger.Information("Extension deactivated: Setting AppDomain for restart because assembly {0} is loaded", extension.Name);
                    ctx.RestartAppDomain = true;
                }
            }
        }

        public override void Monitor(ExtensionDescriptor descriptor, Action<IVolatileToken> monitor) {
            string assemblyPath = GetAssemblyPath(descriptor);
            if (assemblyPath != null) {
                Logger.Information("Monitoring virtual path \"{0}\"", assemblyPath);
                monitor(_virtualPathMonitor.WhenPathChanges(assemblyPath));
            }
        }

        public override ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            var assemblyPath = GetAssemblyPath(descriptor);
            if (assemblyPath == null)
                return null;

            return new ExtensionProbeEntry {
                Descriptor = descriptor,
                LastModificationTimeUtc = File.GetLastWriteTimeUtc(_virtualPathProvider.MapPath(assemblyPath)),
                Loader = this,
                VirtualPath = assemblyPath
            };
        }

        public override ExtensionEntry Load(ExtensionDescriptor descriptor) {
            var dependency = _dependenciesFolder.GetDescriptor(descriptor.Name);
            if (dependency != null && dependency.LoaderName == this.Name) {

                var assembly = _assemblyProbingFolder.LoadAssembly(descriptor.Name);
                if (assembly == null)
                    return null;

                return new ExtensionEntry {
                    Descriptor = descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.GetExportedTypes()
                };
            }
            return null;
        }

        public string GetAssemblyPath(ExtensionDescriptor descriptor) {
            var assemblyPath = _virtualPathProvider.Combine(descriptor.Location, descriptor.Name, "bin",
                                                            descriptor.Name + ".dll");
            if (!_virtualPathProvider.FileExists(assemblyPath))
                return null;

            return assemblyPath;
        }
    }
}