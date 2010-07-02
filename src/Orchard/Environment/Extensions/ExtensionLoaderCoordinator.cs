using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;
using Orchard.FileSystems.VirtualPath;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Utility;

namespace Orchard.Environment.Extensions {
    public class ExtensionLoaderCoordinator : IExtensionLoaderCoordinator {
        private readonly IDependenciesFolder _dependenciesFolder;
        private readonly IExtensionManager _extensionManager;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IVirtualPathMonitor _virtualPathMonitor;
        private readonly IEnumerable<IExtensionLoader> _loaders;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IBuildManager _buildManager;

        public ExtensionLoaderCoordinator(
            IDependenciesFolder dependenciesFolder,
            IExtensionManager extensionManager,
            IVirtualPathProvider virtualPathProvider,
            IVirtualPathMonitor virtualPathMonitor,
            IEnumerable<IExtensionLoader> loaders,
            IHostEnvironment hostEnvironment,
            IBuildManager buildManager) {

            _dependenciesFolder = dependenciesFolder;
            _extensionManager = extensionManager;
            _virtualPathProvider = virtualPathProvider;
            _virtualPathMonitor = virtualPathMonitor;
            _loaders = loaders.OrderBy(l => l.Order);
            _hostEnvironment = hostEnvironment;
            _buildManager = buildManager;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public void SetupExtensions() {
            Logger.Information("Start loading extensions...");

            var context = CreateLoadingContext();

            // Notify all loaders about extensions removed from the web site
            foreach (var dependency in context.DeletedDependencies) {
                Logger.Information("Extension {0} has been removed from site", dependency.Name);
                foreach (var loader in _loaders) {
                    if (dependency.LoaderName == loader.Name) {
                        loader.ExtensionRemoved(context, dependency);
                    }
                }
            }

            // For all existing extensions in the site, ask each loader if they can
            // load that extension.
            foreach (var extension in context.AvailableExtensions) {
                ProcessExtension(context, extension);
            }

            // Execute all the work need by "ctx"
            ProcessContextCommands(context);

            // And finally save the new entries in the dependencies folder
            _dependenciesFolder.StoreDescriptors(context.NewDependencies);

            Logger.Information("Done loading extensions...");

            // Very last step: Notify the host environment to restart the AppDomain if needed
            if (context.ResetSiteCompilation) {
                Logger.Information("Reset site compilation state required.");
                _hostEnvironment.ResetSiteCompilation();
            }

            if (context.RestartAppDomain) {
                Logger.Information("AppDomain restart required.");
                _hostEnvironment.RestartAppDomain();
            }
        }

        private void ProcessExtension(ExtensionLoadingContext context, ExtensionDescriptor extension) {

            var extensionProbes = context.AvailableExtensionsProbes.ContainsKey(extension.Name) ?
                context.AvailableExtensionsProbes[extension.Name] :
                Enumerable.Empty<ExtensionProbeEntry>();

            if (Logger.IsEnabled(LogLevel.Debug)) {
                Logger.Debug("Loaders for extension \"{0}\": ", extension.Name);
                foreach (var probe in extensionProbes) {
                    Logger.Debug("  Loader: {0}", probe.Loader.Name);
                    Logger.Debug("    VirtualPath: {0}", probe.VirtualPath);
                    Logger.Debug("    DateTimeUtc: {0}", probe.LastModificationTimeUtc);
                }
            }

            var moduleReferences =
                context.AvailableExtensions
                    .Where(e =>
                           context.ReferencesByModule.ContainsKey(extension.Name) &&
                           context.ReferencesByModule[extension.Name].Any(r => StringComparer.OrdinalIgnoreCase.Equals(e.Name, r.Name)));

            var processedModuleReferences =
                moduleReferences
                .Where(e => context.ProcessedExtensions.ContainsKey(e.Name))
                .Select(e => context.ProcessedExtensions[e.Name]);

            var activatedExtension = extensionProbes
                .Where(e => e.Loader.IsCompatibleWithReferences(extension, processedModuleReferences))
                .FirstOrDefault();

            var previousDependency = context.PreviousDependencies.Where(d => StringComparer.OrdinalIgnoreCase.Equals(d.Name, extension.Name)).FirstOrDefault();

            if (activatedExtension == null) {
                Logger.Warning("No loader found for extension \"{0}\"!", extension.Name);
            }

            var references = ProcessExtensionReferences(context, activatedExtension);

            foreach (var loader in _loaders) {
                if (activatedExtension != null && activatedExtension.Loader.Name == loader.Name) {
                    Logger.Information("Activating extension \"{0}\" with loader \"{1}\"", activatedExtension.Descriptor.Name, loader.Name);
                    loader.ExtensionActivated(context, extension);
                }
                else if (previousDependency != null && previousDependency.LoaderName == loader.Name) {
                    Logger.Information("Deactivating extension \"{0}\" from loader \"{1}\"", previousDependency.Name, loader.Name);
                    loader.ExtensionDeactivated(context, extension);
                }
            }

            if (activatedExtension != null) {
                context.NewDependencies.Add(new DependencyDescriptor {
                    Name = extension.Name,
                    LoaderName = activatedExtension.Loader.Name,
                    VirtualPath = activatedExtension.VirtualPath,
                    References = references
                });
            }

            // Keep track of which loader we use for every extension
            // This will be needed for processing references from other dependent extensions
            context.ProcessedExtensions.Add(extension.Name, activatedExtension);
        }

        private ExtensionLoadingContext CreateLoadingContext() {
            var availableExtensions = _extensionManager
                .AvailableExtensions()
                .Where(d => d.ExtensionType == "Module")
                .OrderBy(d => d.Name)
                .ToList();

            var previousDependencies = _dependenciesFolder.LoadDescriptors().ToList();

            var availableExtensionsProbes = availableExtensions.SelectMany(extension => _loaders
                                                                                            .Select(loader => loader.Probe(extension))
                                                                                            .Where(probe => probe != null))
                .GroupBy(e => e.Descriptor.Name)
                .ToDictionary(g => g.Key, g => g.AsEnumerable()
                                                   .OrderByDescending(probe => probe.LastModificationTimeUtc)
                                                   .ThenBy(probe => probe.Loader.Order), StringComparer.OrdinalIgnoreCase);

            var deletedDependencies = previousDependencies
                .Where(e => !availableExtensions.Any(e2 => StringComparer.OrdinalIgnoreCase.Equals(e2.Name, e.Name)))
                .ToList();

            // Collect references for all modules
            var references =
                availableExtensions
                    .SelectMany(extension => _loaders.SelectMany(loader => loader.ProbeReferences(extension)))
                    .ToList();

            var referencesByModule = references
                .GroupBy(entry => entry.Descriptor.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.AsEnumerable(), StringComparer.OrdinalIgnoreCase);

            var referencesByName = references
                .GroupBy(reference => reference.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.AsEnumerable(), StringComparer.OrdinalIgnoreCase);

            var sortedAvailableExtensions =
                availableExtensions.OrderByDependencies(
                    (item, dep) => referencesByModule.ContainsKey(item.Name) &&
                                   referencesByModule[item.Name].Any(r => StringComparer.OrdinalIgnoreCase.Equals(dep.Name, r.Name)))
                    .ToList();

            return new ExtensionLoadingContext {
                AvailableExtensions = sortedAvailableExtensions,
                PreviousDependencies = previousDependencies,
                DeletedDependencies = deletedDependencies,
                AvailableExtensionsProbes = availableExtensionsProbes,
                ReferencesByName = referencesByName,
                ReferencesByModule = referencesByModule
            };
        }

        IEnumerable<DependencyReferenceDescriptor> ProcessExtensionReferences(ExtensionLoadingContext context, ExtensionProbeEntry activatedExtension) {
            if (activatedExtension == null)
                return Enumerable.Empty<DependencyReferenceDescriptor>();

            var referenceNames = (context.ReferencesByModule.ContainsKey(activatedExtension.Descriptor.Name) ?
                context.ReferencesByModule[activatedExtension.Descriptor.Name] :
                Enumerable.Empty<ExtensionReferenceProbeEntry>())
                .Select(r => r.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            var referencesDecriptors = new List<DependencyReferenceDescriptor>();
            foreach (var referenceName in referenceNames) {
                ProcessExtensionReference(context, activatedExtension, referenceName, referencesDecriptors);
            }

            return referencesDecriptors;
        }

        private void ProcessExtensionReference(ExtensionLoadingContext context,
            ExtensionProbeEntry activatedExtension,
            string referenceName,
            IList<DependencyReferenceDescriptor> activatedReferences) {

            // Assemblies loaded by the BuildManager are ignored, since
            // we don't want to update them and they are automatically
            // referenced by the build manager
            if (_buildManager.GetReferencedAssemblies().Any(a => StringComparer.OrdinalIgnoreCase.Equals(a.GetName().Name, referenceName)))
                return;

            var references = context.ReferencesByName.ContainsKey(referenceName) ?
                context.ReferencesByName[referenceName] :
                Enumerable.Empty<ExtensionReferenceProbeEntry>();

            // Binary references
            var bestBinaryReference = references
                .Where(entry => !string.IsNullOrEmpty(entry.VirtualPath))
                .Select(entry => new { Entry = entry, LastWriteTimeUtc = _virtualPathProvider.GetFileLastWriteTimeUtc(entry.VirtualPath) })
                .OrderBy(e => e.LastWriteTimeUtc)
                .ThenBy(e => e.Entry.Name).FirstOrDefault();

            var bestProbe = context.ProcessedExtensions.ContainsKey(referenceName) ?
                context.ProcessedExtensions[referenceName] :
                null;

            // Pick the best one of module vs binary
            if (bestProbe != null && bestBinaryReference != null) {
                if (bestProbe.LastModificationTimeUtc >= bestBinaryReference.LastWriteTimeUtc) {
                    bestBinaryReference = null;
                }
                else {
                    bestProbe = null;
                }
            }

            // Activate the binary ref
            if (bestBinaryReference != null) {
                if (!context.ProcessedReferences.Contains(bestBinaryReference.Entry.Name)) {
                    context.ProcessedReferences.Add(bestBinaryReference.Entry.Name);
                    bestBinaryReference.Entry.Loader.ReferenceActivated(context, bestBinaryReference.Entry);
                }
                activatedReferences.Add(new DependencyReferenceDescriptor {
                    LoaderName = bestBinaryReference.Entry.Loader.Name,
                    Name = bestBinaryReference.Entry.Name,
                    VirtualPath = bestBinaryReference.Entry.VirtualPath
                });
                return;
            }

            // Activated the module ref
            if (bestProbe != null) {
                activatedReferences.Add(new DependencyReferenceDescriptor {
                    LoaderName = bestProbe.Loader.Name,
                    Name = referenceName,
                    VirtualPath = bestProbe.VirtualPath
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
        }

        public void MonitorExtensions(Action<IVolatileToken> monitor) {
            // Monitor add/remove of any module
            monitor(_virtualPathMonitor.WhenPathChanges("~/Modules"));

            // Give loaders a chance to monitor any additional changes
            var extensions = _extensionManager.AvailableExtensions().Where(d => d.ExtensionType == "Module").ToList();
            foreach (var extension in extensions) {
                foreach (var loader in _loaders) {
                    loader.Monitor(extension, monitor);
                }
            }
        }
    }
}
