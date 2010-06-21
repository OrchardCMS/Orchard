using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.Environment.Extensions {
    public class ExtensionLoaderCoordinator : IExtensionLoaderCoordinator {
        private readonly IDependenciesFolder _dependenciesFolder;
        private readonly IExtensionManager _extensionManager;
        private readonly IEnumerable<IExtensionLoader> _loaders;
        private readonly IHostEnvironment _hostEnvironment;

        public ExtensionLoaderCoordinator(
            IDependenciesFolder dependenciesFolder,
            IExtensionManager extensionManager,
            IEnumerable<IExtensionLoader> loaders,
            IHostEnvironment hostEnvironment) {

            _dependenciesFolder = dependenciesFolder;
            _extensionManager = extensionManager;
            _loaders = loaders.OrderBy(l => l.Order);
            _hostEnvironment = hostEnvironment;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public void SetupExtensions() {
            Logger.Information("Loading extensions.");

            var extensions = _extensionManager.AvailableExtensions().Where(d => d.ExtensionType == "Module").ToList();
            var existingDependencies = _dependenciesFolder.LoadDescriptors().ToList();
            var deletedDependencies = existingDependencies.Where(e => !extensions.Any(e2 => e2.Name == e.Name)).ToList();

            var loadingContext = new ExtensionLoadingContext();

            // Notify all loaders about extensions removed from the web site
            foreach (var dependency in deletedDependencies) {
                Logger.Information("Extension {0} has been removed from site", dependency.Name);
                foreach (var loader in _loaders) {
                    if (dependency.LoaderName == loader.Name) {
                        loader.ExtensionRemoved(loadingContext, dependency);
                    }
                }
            }

            // For all existing extensions in the site, ask each loader if they can
            // load that extension.
            var newDependencies = new List<DependencyDescriptor>();
            foreach (var extension in extensions) {
                ProcessExtension(loadingContext, extension, existingDependencies, newDependencies);
            }

            // Execute all the work need by "ctx"
            ProcessContextCommands(loadingContext);

            // And finally save the new entries in the dependencies folder
            _dependenciesFolder.StoreDescriptors(newDependencies);
            Logger.Information("Done loading extensions.");
        }

        private void ProcessExtension(
            ExtensionLoadingContext loadingContext,
            ExtensionDescriptor extension,
            IEnumerable<DependencyDescriptor> existingDependencies,
            List<DependencyDescriptor> newDependencies) {

            var extensionProbes = _loaders
                .Select(loader => loader.Probe(extension))
                .Where(probe => probe != null)
                .OrderByDescending(probe => probe.LastModificationTimeUtc)
                .ThenBy(probe => probe.Loader.Order)
                .ToList();


            if (Logger.IsEnabled(LogLevel.Debug)) {
                Logger.Debug("Loaders for extension \"{0}\": ", extension.Name);
                foreach (var probe in extensionProbes) {
                    Logger.Debug("  Loader: {0}", probe.Loader.Name);
                    Logger.Debug("    VirtualPath: {0}", probe.VirtualPath);
                    Logger.Debug("    DateTimeUtc: {0}", probe.LastModificationTimeUtc);
                }
            }

            var activatedExtension = extensionProbes.FirstOrDefault();
            var previousDependency = existingDependencies.Where(d => d.Name == extension.Name).FirstOrDefault();

            if (activatedExtension == null) {
                Logger.Warning("No loader found for extension \"{0}\"!", extension.Name);
            }

            foreach (var loader in _loaders) {
                if (activatedExtension != null && activatedExtension.Loader.Name == loader.Name) {
                    Logger.Information("Activating extension \"{0}\" with loader \"{1}\"", activatedExtension.Descriptor.Name, loader.Name);
                    loader.ExtensionActivated(loadingContext, extension);
                }
                else if (previousDependency != null && previousDependency.LoaderName == loader.Name) {
                    Logger.Information("Deactivating extension \"{0}\" from loader \"{1}\"", previousDependency.Name, loader.Name);
                    loader.ExtensionDeactivated(loadingContext, extension);
                }
            }

            if (activatedExtension != null) {
                newDependencies.Add(new DependencyDescriptor {
                    Name = extension.Name,
                    LoaderName = activatedExtension.Loader.Name,
                    VirtualPath = activatedExtension.VirtualPath
                });
            }
        }

        private void ProcessContextCommands(ExtensionLoadingContext ctx) {
            Logger.Information("Executing list of operations needed for loading extensions...");
            foreach (var action in ctx.DeleteActions) {
                action();
            }

            foreach (var action in ctx.CopyActions) {
                action();
            }

            if (ctx.RestartAppDomain) {
                Logger.Information("AppDomain restart required.");
                _hostEnvironment.RestartAppDomain();
            }

            if (ctx.ResetSiteCompilation) {
                Logger.Information("Reset site compilation state required.");
                _hostEnvironment.ResetSiteCompilation();
            }
        }

        public void MonitorExtensions(Action<IVolatileToken> monitor) {
            var extensions = _extensionManager.AvailableExtensions().Where(d => d.ExtensionType == "Module").ToList();
            foreach (var extension in extensions) {
                foreach (var loader in _loaders) {
                    loader.Monitor(extension, monitor);
                }
            }
        }
    }
}
