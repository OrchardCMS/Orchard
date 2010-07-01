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

namespace Orchard.Environment.Extensions.Loaders {
    public class DynamicExtensionLoader : ExtensionLoaderBase {
        private readonly IBuildManager _buildManager;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IVirtualPathMonitor _virtualPathMonitor;
        private readonly IProjectFileParser _projectFileParser;
        private static readonly ReloadWorkaround _reloadWorkaround = new ReloadWorkaround();

        public DynamicExtensionLoader(
            IBuildManager buildManager,
            IVirtualPathProvider virtualPathProvider,
            IVirtualPathMonitor virtualPathMonitor,
            IDependenciesFolder dependenciesFolder,
            IProjectFileParser projectFileParser)
            : base(dependenciesFolder) {

            _buildManager = buildManager;
            _virtualPathProvider = virtualPathProvider;
            _virtualPathMonitor = virtualPathMonitor;
            _projectFileParser = projectFileParser;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public override int Order { get { return 100; } }

        public override string GetWebFormAssemblyDirective(DependencyDescriptor dependency) {
            return string.Format("<%@ Assembly Src=\"{0}\"%>", dependency.VirtualPath);
        }

        public override IEnumerable<string> GetWebFormVirtualDependencies(DependencyDescriptor dependency) {
            yield return dependency.VirtualPath;
        }

        public override void Monitor(ExtensionDescriptor descriptor, Action<IVolatileToken> monitor) {
            // We need to monitor the path to the ".csproj" file.
            string projectPath = GetProjectPath(descriptor);
            if (projectPath != null) {
                Logger.Information("Monitoring virtual path \"{0}\"", projectPath);
                monitor(_virtualPathMonitor.WhenPathChanges(projectPath));
                _reloadWorkaround.Monitor(_virtualPathMonitor.WhenPathChanges(projectPath));
            }
        }

        public override void ExtensionRemoved(ExtensionLoadingContext ctx, DependencyDescriptor dependency) {
            // Since a dynamic assembly is not active anymore, we need to notify ASP.NET
            // that a new site compilation is needed (since ascx files may be referencing
            // this now removed extension).
            Logger.Information("ExtensionRemoved: Module \"{0}\" has been removed, forcing site recompilation");
            ctx.ResetSiteCompilation = true;
        }

        public override void ExtensionDeactivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
            // Since a dynamic assembly is not active anymore, we need to notify ASP.NET
            // that a new site compilation is needed (since ascx files may be referencing
            // this now removed extension).
            Logger.Information("ExtensionDeactivated: Module \"{0}\" has been de-activated, forcing site recompilation");
            ctx.ResetSiteCompilation = true;
        }

        public override void ExtensionActivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
            if (_reloadWorkaround.AppDomainRestartNeeded) {
                Logger.Information("ExtensionActivated: Module \"{0}\" has changed, forcing AppDomain restart", extension.Name);
                ctx.RestartAppDomain = _reloadWorkaround.AppDomainRestartNeeded;
            }
        }

        public override IEnumerable<ExtensionReferenceProbeEntry> ProbeReferences(ExtensionDescriptor descriptor) {
            string projectPath = GetProjectPath(descriptor);
            if (projectPath == null)
                return Enumerable.Empty<ExtensionReferenceProbeEntry>();

            using(var stream = _virtualPathProvider.OpenFile(projectPath)) {
                var projectFile = _projectFileParser.Parse(stream);

                return projectFile.References.Select(r => new ExtensionReferenceProbeEntry {
                    Descriptor = descriptor,
                    Loader = this,
                    Name = r.AssemblyName,
                    VirtualPath = null
                });
            }
        }

        public override Assembly LoadReference(DependencyReferenceDescriptor reference) {
            return _buildManager.GetCompiledAssembly(reference.VirtualPath);
        }

        public override ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
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

        protected override ExtensionEntry LoadWorker(ExtensionDescriptor descriptor) {
            string projectPath = GetProjectPath(descriptor);
            if (projectPath == null)
                return null;

            var assembly = _buildManager.GetCompiledAssembly(projectPath);
            if (assembly == null)
                return null;
            //Logger.Information("Loading extension \"{0}\": assembly name=\"{1}\"", descriptor.Name, assembly.GetName().Name);

            return new ExtensionEntry {
                Descriptor = descriptor,
                Assembly = assembly,
                ExportedTypes = assembly.GetExportedTypes(),
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

        /// <summary>
        /// We should be able to support reloading multiple version of a compiled module from
        /// a ".csproj" file in the same AppDomain. However, we are currently running into a 
        /// limitation with NHibernate getting confused when a type name is present in
        /// multiple assemblies loaded in an AppDomain.  So, until a solution is found, any change 
        /// to a ".csproj" file of an active module requires an AppDomain restart.
        /// The purpose of this class is to keep track of all .csproj files monitored until
        /// an AppDomain restart.
        /// </summary>
        class ReloadWorkaround {
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