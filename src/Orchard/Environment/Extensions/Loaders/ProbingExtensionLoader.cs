using System.Collections.Generic;
using System.Linq;
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
                ctx.DeleteActions.Add(
                    () => {
                        Logger.Information("ExtensionRemoved: Deleting assembly \"{0}\" from probing directory", dependency.Name);
                        _assemblyProbingFolder.DeleteAssembly(dependency.Name);
                    });

                // We need to restart the appDomain if the assembly is loaded
                if (IsAssemblyLoaded(dependency.Name)) {
                    Logger.Information("ExtensionRemoved: Module \"{0}\" is removed and its assembly is loaded, forcing AppDomain restart", dependency.Name);
                    ctx.RestartAppDomain = true;
                }
            }
        }

        public override void ExtensionDeactivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
            if (_assemblyProbingFolder.AssemblyExists(extension.Name)) {
                ctx.DeleteActions.Add(
                    () => {
                        Logger.Information("ExtensionDeactivated: Deleting assembly \"{0}\" from probing directory", extension.Name);
                        _assemblyProbingFolder.DeleteAssembly(extension.Name);
                    });

                // We need to restart the appDomain if the assembly is loaded
                if (IsAssemblyLoaded(extension.Name)) {
                    Logger.Information("ExtensionDeactivated: Module \"{0}\" is deactivated and its assembly is loaded, forcing AppDomain restart", extension.Name);
                    ctx.RestartAppDomain = true;
                }
            }
        }

        public override bool IsCompatibleWithReferences(ExtensionDescriptor extension, IEnumerable<ExtensionProbeEntry> references) {
            // A pre-compiled module is _not_ compatible with a dynamically loaded module
            // because a pre-compiled module usually references a pre-compiled assembly binary
            // which will have a different identity (i.e. name) from the dynamic module.
            bool result = references.All(r => r.Loader.GetType() != typeof(DynamicExtensionLoader));
            if (!result) {
                Logger.Information("Extension \"{0}\" will not be loaded as pre-compiled extension because one or more referenced extension is dynamically compiled", extension.Name);
            }
            return result;
        }

        public override ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            if (!_assemblyProbingFolder.AssemblyExists(descriptor.Name))
                return null;

            var desc = _dependenciesFolder.GetDescriptor(descriptor.Name);
            if (desc == null)
                return null;

            return new ExtensionProbeEntry {
                Descriptor = descriptor,
                LastWriteTimeUtc = _assemblyProbingFolder.GetAssemblyDateTimeUtc(descriptor.Name),
                Loader = this,
                VirtualPath = desc.VirtualPath
            };
        }

        protected override ExtensionEntry LoadWorker(ExtensionDescriptor descriptor) {
            var assembly = _assemblyProbingFolder.LoadAssembly(descriptor.Name);
            if (assembly == null)
                return null;

            //Logger.Information("Loading extension \"{0}\"", descriptor.Name);

            return new ExtensionEntry {
                Descriptor = descriptor,
                Assembly = assembly,
                ExportedTypes = assembly.GetExportedTypes()
            };
        }
    }
}