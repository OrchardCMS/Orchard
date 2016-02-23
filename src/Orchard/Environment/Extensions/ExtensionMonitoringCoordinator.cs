using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;

namespace Orchard.Environment.Extensions {
    public class ExtensionMonitoringCoordinator : IExtensionMonitoringCoordinator {
        private readonly IVirtualPathMonitor _virtualPathMonitor;
        private readonly IAsyncTokenProvider _asyncTokenProvider;
        private readonly IExtensionManager _extensionManager;
        private readonly IEnumerable<IExtensionLoader> _loaders;

        public ExtensionMonitoringCoordinator(
            IVirtualPathMonitor virtualPathMonitor,
            IAsyncTokenProvider asyncTokenProvider,
            IExtensionManager extensionManager,
            IEnumerable<IExtensionLoader> loaders) {

            _virtualPathMonitor = virtualPathMonitor;
            _asyncTokenProvider = asyncTokenProvider;
            _extensionManager = extensionManager;
            _loaders = loaders;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public bool Disabled { get; set; }

        public void MonitorExtensions(Action<IVolatileToken> monitor) {
            // We may be disabled by custom host configuration for performance reasons
            if (Disabled)
                return;

            //PERF: Monitor extensions asynchronously.
            monitor(_asyncTokenProvider.GetToken(MonitorExtensionsWork));
        }

        public void MonitorExtensionsWork(Action<IVolatileToken> monitor) {
            var locations = _extensionManager.AvailableExtensions().Select(e => e.Location).Distinct(StringComparer.InvariantCultureIgnoreCase);

            Logger.Information("Start monitoring extension files...");
            // Monitor add/remove of any module/theme
            foreach(string location in locations) {
                Logger.Debug("Monitoring virtual path \"{0}\"", location);
                monitor(_virtualPathMonitor.WhenPathChanges(location));
            }

            // Give loaders a chance to monitor any additional changes
            var extensions = _extensionManager.AvailableExtensions().Where(d => DefaultExtensionTypes.IsModule(d.ExtensionType) || DefaultExtensionTypes.IsTheme(d.ExtensionType)).ToList();
            foreach (var extension in extensions) {
                foreach (var loader in _loaders) {
                    loader.Monitor(extension, monitor);
                }
            }
            Logger.Information("Done monitoring extension files...");
        }
    }
}