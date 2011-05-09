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

        public override string GetWebFormAssemblyDirective(DependencyDescriptor dependency) {
            return string.Format("<%@ Assembly Src=\"{0}\"%>", dependency.VirtualPath);
        }

        public override IEnumerable<string> GetWebFormVirtualDependencies(DependencyDescriptor dependency) {
            // Return csproj and all .cs files
            return GetDependencies(dependency.VirtualPath);
        }

        public override IEnumerable<string> GetDynamicModuleDependencies(DependencyDescriptor dependency, string virtualPath) {
            virtualPath = _virtualPathProvider.ToAppRelative(virtualPath);

            if (StringComparer.OrdinalIgnoreCase.Equals(virtualPath, dependency.VirtualPath)) {
                return GetDependencies(virtualPath);
            }
            return base.GetDynamicModuleDependencies(dependency, virtualPath);
        }

        public override void Monitor(ExtensionDescriptor descriptor, Action<IVolatileToken> monitor) {
            if (Disabled)
                return;

            // Monitor .csproj and all .cs files
            string projectPath = GetProjectPath(descriptor);
            if (projectPath != null) {
                foreach (var path in GetDependencies(projectPath)) {
                    Logger.Information("Monitoring virtual path \"{0}\"", path);

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
            string projectPath = GetProjectPath(descriptor);
            if (projectPath == null)
                return Enumerable.Empty<ExtensionReferenceProbeEntry>();

            var projectFile = _projectFileParser.Parse(projectPath);

                return projectFile.References.Select(r => new ExtensionReferenceProbeEntry {
                    Descriptor = descriptor,
                    Loader = this,
                    Name = r.SimpleName,
                    VirtualPath = _virtualPathProvider.GetProjectReferenceVirtualPath(projectPath, r.SimpleName, r.Path)
                });
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
            // DynamicExtensionLoader has 2 types of references: assemblies from module bin directory
            // and .csproj.
            if (StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(reference.VirtualPath), ".dll"))
                return _assemblyProbingFolder.LoadAssembly(reference.Name);

            return _buildManager.GetCompiledAssembly(reference.VirtualPath);
        }

        public override ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            if (Disabled)
                return null;

            string projectPath = GetProjectPath(descriptor);
            if (projectPath == null)
                return null;

            return new ExtensionProbeEntry {
                Descriptor = descriptor,
                LastWriteTimeUtc = GetDependencies(projectPath).Max(f => _virtualPathProvider.GetFileLastWriteTimeUtc(f)),
                Loader = this,
                VirtualPath = projectPath
            };
        }

        protected override ExtensionEntry LoadWorker(ExtensionDescriptor descriptor) {
            if (Disabled)
                return null;

            string projectPath = GetProjectPath(descriptor);
            if (projectPath == null)
                return null;

            var assembly = _buildManager.GetCompiledAssembly(projectPath);
            if (assembly == null)
                return null;

            Logger.Information("Loaded dynamic extension \"{0}\": assembly name=\"{1}\"", descriptor.Name, assembly.FullName);

            return new ExtensionEntry {
                Descriptor = descriptor,
                Assembly = assembly,
                ExportedTypes = assembly.GetExportedTypes(),
            };
        }

        protected IEnumerable<string> GetDependencies(string projectPath) {
            var dependencies = new HashSet<string> { projectPath };

            AddDependencies(projectPath, dependencies);

            return dependencies;
        }

        private void AddDependencies(string projectPath, HashSet<string> currentSet) {
            string basePath = _virtualPathProvider.GetDirectoryName(projectPath);

            ProjectFileDescriptor projectFile = _projectFileParser.Parse(projectPath);

                // Add source files
                currentSet.UnionWith(projectFile.SourceFilenames.Select(f => _virtualPathProvider.Combine(basePath, f)));

                // Add Project and Library references
                if (projectFile.References != null) {
                    foreach (ReferenceDescriptor referenceDescriptor in projectFile.References.Where(reference => !string.IsNullOrEmpty(reference.Path))) {
                        string path = referenceDescriptor.ReferenceType == ReferenceType.Library
                                          ? _virtualPathProvider.GetProjectReferenceVirtualPath(projectPath, referenceDescriptor.SimpleName, referenceDescriptor.Path)
                                          : _virtualPathProvider.Combine(basePath, referenceDescriptor.Path);

                        // Normalize the virtual path (avoid ".." in the path name)
                        if (!string.IsNullOrEmpty(path)) {
                            try {
                                path = _virtualPathProvider.ToAppRelative(path);
                            }
                            catch (Exception e) {
                                // The initial path might have been invalid (e.g. path indicates a path outside the application root)
                                Logger.Information(e, "Path '{0}' cannot be made app relative", path);
                                path = null;
                            }
                        }

                        // Attempt to reference the project / library file
                        if (!string.IsNullOrEmpty(path) && !currentSet.Contains(path) && _virtualPathProvider.TryFileExists(path)) {
                            currentSet.Add(path);

                            // In case of project, also reference the source files
                            if (referenceDescriptor.ReferenceType == ReferenceType.Project) {
                                AddDependencies(path, currentSet);

                                // Try to also reference any pre-built DLL
                                DependencyDescriptor dependencyDescriptor = _dependenciesFolder.GetDescriptor(_virtualPathProvider.GetDirectoryName(referenceDescriptor.Path));
                                if (dependencyDescriptor != null && _virtualPathProvider.TryFileExists(dependencyDescriptor.VirtualPath)) {
                                    currentSet.Add(dependencyDescriptor.VirtualPath);
                                }
                            }
                        }
                    }
                }
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
                lock(_tokens) {
                    _tokens.Add(whenProjectFileChanges);
                }
            }

            public bool AppDomainRestartNeeded {
                get {
                    lock(_tokens) {
                        return _tokens.Any(t => t.IsCurrent == false);
                    }
                }
            }
        }
    }
}