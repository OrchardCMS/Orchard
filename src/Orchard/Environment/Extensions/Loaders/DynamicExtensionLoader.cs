using System;
using System.Collections.Generic;
using System.IO;
using Orchard.Caching;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;

namespace Orchard.Environment.Extensions.Loaders {
    public class DynamicExtensionLoader : ExtensionLoaderBase {
        private readonly IBuildManager _buildManager;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IVirtualPathMonitor _virtualPathMonitor;
        private readonly IDependenciesFolder _dependenciesFolder;

        public DynamicExtensionLoader(
            IBuildManager buildManager,
            IVirtualPathProvider virtualPathProvider,
            IVirtualPathMonitor virtualPathMonitor,
            IDependenciesFolder dependenciesFolder) {

            _buildManager = buildManager;
            _virtualPathProvider = virtualPathProvider;
            _virtualPathMonitor = virtualPathMonitor;
            _dependenciesFolder = dependenciesFolder;
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
                Logger.Information("Monitoring virtual path {0}", projectPath);
                monitor(_virtualPathMonitor.WhenPathChanges(projectPath));
            }
        }

        public override void ExtensionRemoved(ExtensionLoadingContext ctx, DependencyDescriptor dependency) {
            // Since a dynamic assembly is not active anymore, we need to notify ASP.NET
            // that a new site compilation is needed (since ascx files may be referencing
            // this now removed extension).
            ctx.ResetSiteCompilation = true;
        }

        public override void ExtensionDeactivated(ExtensionLoadingContext ctx, bool isNewExtension, ExtensionDescriptor extension) {
            // Since a dynamic assembly is not active anymore, we need to notify ASP.NET
            // that a new site compilation is needed (since ascx files may be referencing
            // this now removed extension).
            ctx.ResetSiteCompilation = true;
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

        public override ExtensionEntry Load(ExtensionDescriptor descriptor) {
            var dependency = _dependenciesFolder.GetDescriptor(descriptor.Name);
            if (dependency != null && dependency.LoaderName == this.Name) {

                var assembly = _buildManager.GetCompiledAssembly(dependency.VirtualPath);
                Logger.Information("Loading extension \"{0}\": assembly name=\"{1}\"", dependency.Name, assembly.GetName().Name);

                return new ExtensionEntry {
                    Descriptor = descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.GetExportedTypes(),
                };
            }
            return null;
        }

        private string GetProjectPath(ExtensionDescriptor descriptor) {
            string projectPath = _virtualPathProvider.Combine(descriptor.Location, descriptor.Name,
                                                       descriptor.Name + ".csproj");

            if (!_virtualPathProvider.FileExists(projectPath)) {
                return null;
            }

            return projectPath;
        }
    }
}