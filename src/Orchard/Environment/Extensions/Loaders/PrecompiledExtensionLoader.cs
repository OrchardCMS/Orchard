using System;
using System.Collections.Generic;
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

        public PrecompiledExtensionLoader(IDependenciesFolder dependenciesFolder, 
            IAssemblyProbingFolder assemblyProbingFolder, 
            IVirtualPathProvider virtualPathProvider,
            IVirtualPathMonitor virtualPathMonitor)
            : base(dependenciesFolder) {

            _dependenciesFolder = dependenciesFolder;
            _assemblyProbingFolder = assemblyProbingFolder;
            _virtualPathProvider = virtualPathProvider;
            _virtualPathMonitor = virtualPathMonitor;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public override int Order { get { return 30; } }

        public override string GetWebFormAssemblyDirective(DependencyDescriptor dependency) {
            return string.Format("<%@ Assembly Name=\"{0}\"%>", dependency.Name);
        }

        public override IEnumerable<string> GetWebFormVirtualDependencies(DependencyDescriptor dependency) {
            yield return _assemblyProbingFolder.GetAssemblyVirtualPath(dependency.Name);
        }

        public override void ExtensionRemoved(ExtensionLoadingContext ctx, DependencyDescriptor dependency) {
            if (_assemblyProbingFolder.AssemblyExists(dependency.Name)) {
                ctx.DeleteActions.Add(() => _assemblyProbingFolder.DeleteAssembly(dependency.Name));

                // We need to restart the appDomain if the assembly is loaded
                if (IsAssemblyLoaded(dependency.Name)) {
                    Logger.Information("Extension removed: Setting AppDomain for restart because assembly {0} is loaded", dependency.Name);
                    ctx.RestartAppDomain = true;
                }
            }
        }

        public override void ExtensionActivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
            string sourceFileName = _virtualPathProvider.MapPath(GetAssemblyPath(extension));

            // Copy the assembly if it doesn't exist or if it is older than the source file.
            bool copyAssembly =
                !_assemblyProbingFolder.AssemblyExists(extension.Name) ||
                File.GetLastWriteTimeUtc(sourceFileName) > _assemblyProbingFolder.GetAssemblyDateTimeUtc(extension.Name);

            if (copyAssembly) {
                ctx.CopyActions.Add(() => _assemblyProbingFolder.StoreAssembly(extension.Name, sourceFileName));
                // We need to restart the appDomain if the assembly is loaded
                if (IsAssemblyLoaded(extension.Name)) {
                    Logger.Information("Extension activated: Setting AppDomain for restart because assembly {0} is loaded", extension.Name);
                    ctx.RestartAppDomain = true;
                }
            }
        }

        public override void ExtensionDeactivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
            if (_assemblyProbingFolder.AssemblyExists(extension.Name)) {
                ctx.DeleteActions.Add(() => _assemblyProbingFolder.DeleteAssembly(extension.Name));

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

        public override ExtensionEntry LoadWorker(ExtensionDescriptor descriptor) {
            var assembly = _assemblyProbingFolder.LoadAssembly(descriptor.Name);
            if (assembly == null)
                return null;

            Logger.Information("Loading extension \"{0}\"", descriptor.Name);

            return new ExtensionEntry {
                Descriptor = descriptor,
                Assembly = assembly,
                ExportedTypes = assembly.GetExportedTypes()
            };
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