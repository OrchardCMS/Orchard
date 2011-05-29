using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Orchard.Caching;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;

namespace Orchard.Environment.Extensions {
    public class ExtensionMonitoringCoordinator : IExtensionMonitoringCoordinator {
        private readonly IVirtualPathMonitor _virtualPathMonitor;
        private readonly IExtensionManager _extensionManager;
        private readonly IEnumerable<IExtensionLoader> _loaders;

        public ExtensionMonitoringCoordinator(IVirtualPathMonitor virtualPathMonitor,
            IExtensionManager extensionManager,
            IEnumerable<IExtensionLoader> loaders) {

            _virtualPathMonitor = virtualPathMonitor;
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

            //PREF: Monitor extensions asynchronously. IsCurrent will be 'true'
            //      until all tokens are collected by the work function.
            var token = new AsyncVolativeToken(MonitorExtensionsWork, Logger);
            monitor(token);
            token.QueueWorkItem();
        }

        public void MonitorExtensionsWork(Action<IVolatileToken> monitor) {
            Logger.Information("Start monitoring extension files...");
            // Monitor add/remove of any module/theme
            monitor(_virtualPathMonitor.WhenPathChanges("~/Modules"));
            monitor(_virtualPathMonitor.WhenPathChanges("~/Themes"));

            // Give loaders a chance to monitor any additional changes
            var extensions = _extensionManager.AvailableExtensions().Where(d => DefaultExtensionTypes.IsModule(d.ExtensionType) || DefaultExtensionTypes.IsTheme(d.ExtensionType)).ToList();
            foreach (var extension in extensions) {
                foreach (var loader in _loaders) {
                    loader.Monitor(extension, monitor);
                }
            }
            Logger.Information("Done monitoring extension files...");
        }

        public class AsyncVolativeToken : IVolatileToken {
            private readonly Action<Action<IVolatileToken>> _task;
            private readonly List<IVolatileToken> _taskTokens = new List<IVolatileToken>();
            private volatile Exception _taskException;
            private volatile bool _isTaskFinished;

            public AsyncVolativeToken(Action<Action<IVolatileToken>> task, ILogger logger) {
                _task = task;
                Logger = logger;
            }

            public ILogger Logger { get; set; }

            public void QueueWorkItem() {
                // Start a work item to collect tokens in our internal array
                ThreadPool.QueueUserWorkItem((state) => {
                    try {
                        _task(token => _taskTokens.Add(token));
                    }
                    catch (Exception e) {
                        Logger.Error(e, "Error while monitoring extension files. Assuming extensions are not current.");
                        _taskException = e;
                    }
                    finally {
                        _isTaskFinished = true;
                    }
                });
            }

            public bool IsCurrent {
                get {
                    // We are current until the task has finished
                    if (_taskException != null) {
                        return false;
                    }
                    if (_isTaskFinished) {
                        return _taskTokens.All(t => t.IsCurrent);
                    }
                    return true;
                }
            }
        }
    }
}