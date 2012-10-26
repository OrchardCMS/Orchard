using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Orchard.Environment.Extensions.Compilers;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;
using Orchard.Utility.Extensions;

namespace Orchard.Environment.Extensions.Loaders {
    /// <summary>
    /// In case <see cref="DynamicExtensionLoader"/> is disabled, this loader will dynamically compile the assembly.
    /// It won't monitor the filesystem.
    /// </summary>
    public class ProbingExtensionLoader : ExtensionLoaderBase {
        public static readonly string[] ExtensionsVirtualPathPrefixes = { "~/Modules/", "~/Themes/" };

        private readonly IBuildManager _buildManager;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IAssemblyProbingFolder _assemblyProbingFolder;
        private readonly IProjectFileParser _projectFileParser;

        public ProbingExtensionLoader(
            IBuildManager buildManager,
            IVirtualPathProvider virtualPathProvider,
            IHostEnvironment hostEnvironment,
            IAssemblyProbingFolder assemblyProbingFolder,
            IDependenciesFolder dependenciesFolder,
            IProjectFileParser projectFileParser)
            : base(dependenciesFolder) {

            _buildManager = buildManager;
            _virtualPathProvider = virtualPathProvider;
            _hostEnvironment = hostEnvironment;
            _assemblyProbingFolder = assemblyProbingFolder;
            _projectFileParser = projectFileParser;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public bool Disabled { get; set; }

        public override int Order { get { return 110; } }

        public override IEnumerable<ExtensionCompilationReference> GetCompilationReferences(DependencyDescriptor dependency) {
            yield return new ExtensionCompilationReference { BuildProviderTarget = dependency.VirtualPath };
        }

        public override void ExtensionRemoved(ExtensionLoadingContext ctx, DependencyDescriptor dependency) {
        }

        public override void ExtensionDeactivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
        }

        public override void ExtensionActivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
        }

        public override IEnumerable<ExtensionReferenceProbeEntry> ProbeReferences(ExtensionDescriptor descriptor) {
            if (Disabled)
                return Enumerable.Empty<ExtensionReferenceProbeEntry>();

            Logger.Information("Probing references for module '{0}'", descriptor.Id);

            string projectPath = GetProjectPath(descriptor);
            if (projectPath == null)
                return Enumerable.Empty<ExtensionReferenceProbeEntry>();

            var projectFile = _projectFileParser.Parse(projectPath);

            var result = projectFile.References.Select(r => new ExtensionReferenceProbeEntry {
                Descriptor = descriptor,
                Loader = this,
                Name = r.SimpleName,
                VirtualPath = _virtualPathProvider.GetProjectReferenceVirtualPath(projectPath, r.SimpleName, r.Path)
            });

            Logger.Information("Done probing references for module '{0}'", descriptor.Id);
            return result;
        }

        public override void ReferenceActivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry) {
            //Note: This is the same implementation as "PrecompiledExtensionLoader"
            if (string.IsNullOrEmpty(referenceEntry.VirtualPath))
                return;

            string sourceFileName = _virtualPathProvider.MapPath(referenceEntry.VirtualPath);

            // Copy the assembly if it doesn't exist or if it is older than the source file.
            bool copyAssembly =
                !_assemblyProbingFolder.AssemblyExists(referenceEntry.Name) ||
                File.GetLastWriteTimeUtc(sourceFileName) > _assemblyProbingFolder.GetAssemblyDateTimeUtc(referenceEntry.Name);

            if (copyAssembly) {
                context.CopyActions.Add(() => _assemblyProbingFolder.StoreAssembly(referenceEntry.Name, sourceFileName));

                // We need to restart the appDomain if the assembly is loaded
                if (_hostEnvironment.IsAssemblyLoaded(referenceEntry.Name)) {
                    Logger.Information("ReferenceActivated: Reference \"{0}\" is activated with newer file and its assembly is loaded, forcing AppDomain restart", referenceEntry.Name);
                    context.RestartAppDomain = true;
                }
            }
        }

        public override Assembly LoadReference(DependencyReferenceDescriptor reference) {
            if (Disabled)
                return null;

            Logger.Information("Loading reference '{0}'", reference.Name);

            // DynamicExtensionLoader has 2 types of references: assemblies from module bin directory
            // and .csproj.
            Assembly result;
            if (StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(reference.VirtualPath), ".dll"))
                result = _assemblyProbingFolder.LoadAssembly(reference.Name);
            else {
                result = CompileAssembly(reference.Name, reference.VirtualPath);
            }

            Logger.Information("Done loading reference '{0}'", reference.Name);
            return result;
        }

        public override ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            if (Disabled)
                return null;

            Logger.Information("Probing for module '{0}'", descriptor.Id);

            string projectPath = GetProjectPath(descriptor);
            if (projectPath == null)
                return null;

            var result = new ExtensionProbeEntry {
                Descriptor = descriptor,
                Loader = this,
                Priority = 50,
                VirtualPath = projectPath,
                VirtualPathDependencies = new string[] { projectPath },
            };

            Logger.Information("Done probing for module '{0}'", descriptor.Id);
            return result;
        }

        protected override ExtensionEntry LoadWorker(ExtensionDescriptor descriptor) {
            if (Disabled)
                return null;

            Logger.Information("Start loading dynamic extension \"{0}\"", descriptor.Name);

            var assembly = _assemblyProbingFolder.LoadAssembly(descriptor.Id);
            if (assembly == null) {
                string projectPath = GetProjectPath(descriptor);
                if (projectPath == null)
                    return null;
                
                assembly = CompileAssembly(descriptor.Id, projectPath);
            }

            if (assembly == null)
                return null;

            Logger.Information("Done loading dynamic extension \"{0}\": assembly name=\"{1}\"", descriptor.Name, assembly.FullName);

            return new ExtensionEntry {
                Descriptor = descriptor,
                Assembly = assembly,
                ExportedTypes = assembly.GetExportedTypes(),
            };
        }

        private Assembly CompileAssembly(string moduleName, string virtualPath) {
            var assembly = _buildManager.GetCompiledAssembly(virtualPath);
            //if (assembly != null) {
            //    _assemblyProbingFolder.StoreAssembly(moduleName, assembly.Location);
            //}

            return assembly;
        }

        private string GetProjectPath(ExtensionDescriptor descriptor) {
            string projectPath = _virtualPathProvider.Combine(descriptor.Location, descriptor.Id,
                                                       descriptor.Id + ".csproj");

            if (!_virtualPathProvider.FileExists(projectPath)) {
                return null;
            }

            return projectPath;
        }
    }
}