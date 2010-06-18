using System.Collections.Generic;
using System.IO;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;
using Orchard.Logging;

namespace Orchard.Environment.Extensions.Loaders {
    /// <summary>
    /// Load an extension using the "Assembly.Load" method if the
    /// file can be found in the "App_Data/Dependencies" folder.
    /// </summary>
    public class ProbingExtensionLoader : ExtensionLoaderBase {
        private readonly IDependenciesFolder _dependenciesFolder;
        private readonly IAssemblyProbingFolder _assemblyProbingFolder;

        public ProbingExtensionLoader(IDependenciesFolder dependenciesFolder, IAssemblyProbingFolder assemblyProbingFolder)
            : base(dependenciesFolder) {

            _dependenciesFolder = dependenciesFolder;
            _assemblyProbingFolder = assemblyProbingFolder;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public override int Order { get { return 40; } }

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

        public override void ExtensionDeactivated(ExtensionLoadingContext ctx, bool isNewExtension, ExtensionDescriptor extension) {
            if (_assemblyProbingFolder.AssemblyExists(extension.Name)) {
                ctx.DeleteActions.Add(() => _assemblyProbingFolder.DeleteAssembly(extension.Name));

                // We need to restart the appDomain if the assembly is loaded
                if (IsAssemblyLoaded(extension.Name)) {
                    Logger.Information("Extension deactivated: Setting AppDomain for restart because assembly {0} is loaded", extension.Name);
                    ctx.RestartAppDomain = true;
                }
            }
        }

        public override ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            if (!_assemblyProbingFolder.AssemblyExists(descriptor.Name))
                return null;

            var desc = _dependenciesFolder.GetDescriptor(descriptor.Name);
            if (desc == null)
                return null;

            return new ExtensionProbeEntry {
                Descriptor = descriptor,
                LastModificationTimeUtc = _assemblyProbingFolder.GetAssemblyDateTimeUtc(descriptor.Name),
                Loader = this,
                VirtualPath = desc.VirtualPath
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
    }
}