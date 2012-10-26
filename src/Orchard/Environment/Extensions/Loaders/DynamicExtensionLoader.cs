using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Orchard.Caching;
using Orchard.Environment.Extensions.Compilers;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;
using Orchard.Utility.Extensions;

namespace Orchard.Environment.Extensions.Loaders {
    public class DynamicExtensionLoader : ExtensionLoaderBase {
        public static readonly string[] ExtensionsVirtualPathPrefixes = { "~/Modules/", "~/Themes/" };

        private readonly IBuildManager _buildManager;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IVirtualPathMonitor _virtualPathMonitor;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IAssemblyProbingFolder _assemblyProbingFolder;
        private readonly IDependenciesFolder _dependenciesFolder;
        private readonly IProjectFileParser _projectFileParser;
        private readonly ReloadWorkaround _reloadWorkaround = new ReloadWorkaround();

        public DynamicExtensionLoader(
            IBuildManager buildManager,
            IVirtualPathProvider virtualPathProvider,
            IVirtualPathMonitor virtualPathMonitor,
            IHostEnvironment hostEnvironment,
            IAssemblyProbingFolder assemblyProbingFolder,
            IDependenciesFolder dependenciesFolder,
            IProjectFileParser projectFileParser)
            : base(dependenciesFolder) {

            _buildManager = buildManager;
            _virtualPathProvider = virtualPathProvider;
            _virtualPathMonitor = virtualPathMonitor;
            _hostEnvironment = hostEnvironment;
            _assemblyProbingFolder = assemblyProbingFolder;
            _projectFileParser = projectFileParser;
            _dependenciesFolder = dependenciesFolder;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public bool Disabled { get; set; }

        public override int Order { get { return 100; } }

        public override IEnumerable<ExtensionCompilationReference> GetCompilationReferences(DependencyDescriptor dependency) {
            yield return new ExtensionCompilationReference { BuildProviderTarget = dependency.VirtualPath };
        }

        public override IEnumerable<string> GetVirtualPathDependencies(DependencyDescriptor dependency) {
            // Return csproj and all .cs files
            return GetDependencies(dependency.VirtualPath);
        }

        public override void Monitor(ExtensionDescriptor descriptor, Action<IVolatileToken> monitor) {
            if (Disabled)
                return;

            // Monitor .csproj and all .cs files
            string projectPath = GetProjectPath(descriptor);
            if (projectPath != null) {
                foreach (var path in GetDependencies(projectPath)) {
                    Logger.Debug("Monitoring virtual path \"{0}\"", path);

                    var token = _virtualPathMonitor.WhenPathChanges(path);
                    monitor(token);
                    _reloadWorkaround.Monitor(token);
                }
            }
        }

        public override void ExtensionRemoved(ExtensionLoadingContext ctx, DependencyDescriptor dependency) {
        }

        public override void ExtensionDeactivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
        }

        public override void ExtensionActivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
            if (_reloadWorkaround.AppDomainRestartNeeded) {
                Logger.Information("ExtensionActivated: Module \"{0}\" has changed, forcing AppDomain restart", extension.Id);
                ctx.RestartAppDomain = _reloadWorkaround.AppDomainRestartNeeded;
            }
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
                result = _buildManager.GetCompiledAssembly(reference.VirtualPath);
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
                VirtualPathDependencies = GetDependencies(projectPath).ToList(),
            };

            Logger.Information("Done probing for module '{0}'", descriptor.Id);
            return result;
        }

        protected override ExtensionEntry LoadWorker(ExtensionDescriptor descriptor) {
            if (Disabled)
                return null;

            string projectPath = GetProjectPath(descriptor);
            if (projectPath == null)
                return null;

            Logger.Information("Start loading dynamic extension \"{0}\"", descriptor.Name);

            var assembly = _buildManager.GetCompiledAssembly(projectPath);
            if (assembly == null)
                return null;

            Logger.Information("Done loading dynamic extension \"{0}\": assembly name=\"{1}\"", descriptor.Name, assembly.FullName);

            return new ExtensionEntry {
                Descriptor = descriptor,
                Assembly = assembly,
                ExportedTypes = assembly.GetExportedTypes(),
            };
        }

        protected IEnumerable<string> GetDependencies(string projectPath) {
            var dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            AddDependencies(projectPath, dependencies);
            return dependencies;
        }

        private void AddDependencies(string projectPath, HashSet<string> currentSet) {
            // Skip files from locations other than "~/Modules" and "~/Themes"
            if (string.IsNullOrEmpty(PrefixMatch(projectPath, ExtensionsVirtualPathPrefixes))) {
                return;
            }

            // Add project path
            currentSet.Add(projectPath);

            // Add source file paths
            var projectFile = _projectFileParser.Parse(projectPath);
            string basePath = _virtualPathProvider.GetDirectoryName(projectPath);
            currentSet.UnionWith(projectFile.SourceFilenames.Select(f => _virtualPathProvider.Combine(basePath, f)));

            // Add Project and Library references
            if (projectFile.References != null) {
                foreach (ReferenceDescriptor referenceDescriptor in projectFile.References.Where(reference => !string.IsNullOrEmpty(reference.Path))) {
                    string path = referenceDescriptor.ReferenceType == ReferenceType.Library
                                      ? _virtualPathProvider.GetProjectReferenceVirtualPath(projectPath, referenceDescriptor.SimpleName, referenceDescriptor.Path)
                                      : _virtualPathProvider.Combine(basePath, referenceDescriptor.Path);

                    // Normalize the virtual path (avoid ".." in the path name)
                    if (!string.IsNullOrEmpty(path)) {
                        path = _virtualPathProvider.ToAppRelative(path);
                    }

                    // Attempt to reference the project / library file
                    if (!string.IsNullOrEmpty(path) && !currentSet.Contains(path) && _virtualPathProvider.TryFileExists(path)) {
                        switch (referenceDescriptor.ReferenceType) {
                            case ReferenceType.Project:
                                AddDependencies(path, currentSet);
                                break;
                            case ReferenceType.Library:
                                currentSet.Add(path);
                                break;
                        }
                    }
                }
            }
        }

        private static string PrefixMatch(string virtualPath, params string[] prefixes) {
            return prefixes
                .FirstOrDefault(p => virtualPath.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }

        private string GetProjectPath(ExtensionDescriptor descriptor) {
            string projectPath = _virtualPathProvider.Combine(descriptor.Location, descriptor.Id,
                                                       descriptor.Id + ".csproj");

            if (!_virtualPathProvider.FileExists(projectPath)) {
                return null;
            }

            return projectPath;
        }

        /// <summary>
        /// We should be able to support reloading multiple version of a compiled module from
        /// a ".csproj" file in the same AppDomain. However, we are currently running into a 
        /// limitation with NHibernate getting confused when a type name is present in
        /// multiple assemblies loaded in an AppDomain.  So, until a solution is found, any change 
        /// to a ".csproj" file of an active module requires an AppDomain restart.
        /// The purpose of this class is to keep track of all .csproj files monitored until
        /// an AppDomain restart.
        /// </summary>
        internal class ReloadWorkaround {
            private readonly List<IVolatileToken> _tokens = new List<IVolatileToken>();

            public void Monitor(IVolatileToken whenProjectFileChanges) {
                lock (_tokens) {
                    _tokens.Add(whenProjectFileChanges);
                }
            }

            public bool AppDomainRestartNeeded {
                get {
                    lock (_tokens) {
                        return _tokens.Any(t => t.IsCurrent == false);
                    }
                }
            }
        }
    }
}