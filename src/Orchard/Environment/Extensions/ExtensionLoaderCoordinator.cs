using System;
using System.Collections.Generic;
using System.IO;
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

            var sameExtensions = extensions.Where(e => existingDependencies.Any(e2 => e2.Name == e.Name)).ToList();
            var deletedDependencies = existingDependencies.Where(e => !extensions.Any(e2 => e2.Name == e.Name)).ToList();
            var newExtensions = extensions.Except(sameExtensions).ToList();

            var ctx = new ExtensionLoadingContext { DependenciesFolder = _dependenciesFolder };

            // Notify all loaders about extensions removed from the web site
            foreach (var dependency in deletedDependencies) {
                Logger.Information("Extension {0} has been removed from site", dependency.Name);
                foreach (var loader in _loaders) {
                    if (dependency.LoaderName == loader.Name) {
                        loader.ExtensionRemoved(ctx, dependency);
                    }
                }
            }

            // For all existing extensions in the site, ask each loader if they can
            // load that extension.
            var newDependencies = new List<DependencyDescriptor>();
            foreach (var extension in extensions) {
                bool isNewExtension = newExtensions.Any(e => e.Name == extension.Name);
                ProcessExtension(ctx, extension, isNewExtension, existingDependencies, newDependencies);
            }

            // Execute all the work need by "ctx"
            ProcessContextCommands(ctx);

            // And finally save the new entries in the dependencies folder
            _dependenciesFolder.StoreDescriptors(newDependencies);
            Logger.Information("Done loading extensions.");
        }

        private void ProcessExtension(
            ExtensionLoadingContext ctx,
            ExtensionDescriptor extension,
            bool isNewExtension,
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
                Logger.Warning("No loader found for extension {0}!", extension.Name);
            }

            foreach (var loader in _loaders) {
                if (activatedExtension != null && activatedExtension.Loader.Name == loader.Name) {
                    Logger.Information("Activating extension \"{0}\" with loader \"{1}\"", activatedExtension.Descriptor.Name, loader.Name);
                    loader.ExtensionActivated(ctx, isNewExtension, extension);
                }
                else if (previousDependency != null && previousDependency.LoaderName == loader.Name) {
                    Logger.Information("Deactivating extension \"{0}\" from loader \"{1}\"", previousDependency.Name, loader.Name);
                    loader.ExtensionDeactivated(ctx, isNewExtension, extension);
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
            foreach (var fileName in ctx.FilesToDelete) {
                Logger.Information("Deleting file \"{0}\"", fileName);
                File.Delete(fileName);
            }
            foreach (var entry in ctx.FilesToCopy) {
                Logger.Information("Copying file from \"{0}\" to \"{1}\"", entry.Key, entry.Value);
                MakeDestinationFileNameAvailable(entry.Value);
                File.Copy(entry.Key, entry.Value);
            }
            foreach (var entry in ctx.FilesToRename) {
                Logger.Information("Moving file from \"{0}\" to \"{1}\"", entry.Key, entry.Value);
                MakeDestinationFileNameAvailable(entry.Value);
                File.Move(entry.Key, entry.Value);
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

        private void MakeDestinationFileNameAvailable(string destinationFileName) {
            // Try deleting the destination first
            try {
                File.Delete(destinationFileName);
            } catch {
                // We land here if the file is in use, for example. Let's move on.
            }

            // If destination doesn't exist, we are good
            if (!File.Exists(destinationFileName))
                return;

            // Try renaming destination to a unique filename
            const string extension = "deleted";
            for (int i = 0; i < 100; i++) {
                var newExtension = (i == 0 ? extension : string.Format("{0}{1}", extension, i));
                var newFileName = Path.ChangeExtension(destinationFileName, newExtension);
                try {
                    File.Delete(newFileName);
                    File.Move(destinationFileName, newFileName);

                    // If successful, we are done...
                    return;
                }
                catch (Exception) {
                    // We need to try with another extension
                }
            }

            // Everything failed, throw an exception
            throw new OrchardException(T("Unable to make room for file {0} in dependencies folder: too many conflicts.", destinationFileName).Text);
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
