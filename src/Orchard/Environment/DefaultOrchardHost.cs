using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Orchard.Caching;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.State;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Utility.Extensions;

namespace Orchard.Environment {
    public class DefaultOrchardHost : IOrchardHost, IShellSettingsManagerEventHandler, IShellDescriptorManagerEventHandler {
        private readonly IHostLocalRestart _hostLocalRestart;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly IRunningShellTable _runningShellTable;
        private readonly IProcessingEngine _processingEngine;
        private readonly IExtensionLoaderCoordinator _extensionLoaderCoordinator;
        private readonly ICacheManager _cacheManager;
        private readonly object _syncLock = new object();

        private IEnumerable<ShellContext> _current;

        public DefaultOrchardHost(
            IShellSettingsManager shellSettingsManager,
            IShellContextFactory shellContextFactory,
            IRunningShellTable runningShellTable,
            IProcessingEngine processingEngine,
            IExtensionLoaderCoordinator extensionLoaderCoordinator,
            ICacheManager cacheManager,
            IHostLocalRestart hostLocalRestart ) {
            _shellSettingsManager = shellSettingsManager;
            _shellContextFactory = shellContextFactory;
            _runningShellTable = runningShellTable;
            _processingEngine = processingEngine;
            _extensionLoaderCoordinator = extensionLoaderCoordinator;
            _cacheManager = cacheManager;
            _hostLocalRestart = hostLocalRestart;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public IList<ShellContext> Current {
            get { return BuildCurrent().ToReadOnlyCollection(); }
        }

        void IOrchardHost.Initialize() {
            Logger.Information("Initializing");
            BuildCurrent();
        }

        void IOrchardHost.ReloadExtensions() {
            DisposeShellContext();
        }

        void IOrchardHost.BeginRequest() {
            Logger.Debug("BeginRequest");
            BeginRequest();
        }

        void IOrchardHost.EndRequest() {
            Logger.Debug("EndRequest");
            EndRequest();
        }

        IWorkContextScope IOrchardHost.CreateStandaloneEnvironment(ShellSettings shellSettings) {
            Logger.Debug("Creating standalone environment for tenant {0}", shellSettings.Name);

            MonitorExtensions();
            BuildCurrent();
            var shellContext = CreateShellContext(shellSettings);
            return shellContext.LifetimeScope.CreateWorkContextScope();
        }

        IEnumerable<ShellContext> BuildCurrent() {
            if (_current == null) {
                lock (_syncLock) {
                    if (_current == null) {
                        SetupExtensions();
                        MonitorExtensions();
                        _current = CreateAndActivate().ToArray();
                    }
                }
            }
            return _current;
        }

        IEnumerable<ShellContext> CreateAndActivate() {
            var allSettings = _shellSettingsManager.LoadSettings();
            if (allSettings.Any()) {
                return allSettings.Select(
                    settings => {
                        var context = CreateShellContext(settings);
                        ActivateShell(context);
                        return context;
                    });
            }

            var setupContext = CreateSetupContext();
            ActivateShell(setupContext);
            return new[] { setupContext };
        }

        private void ActivateShell(ShellContext context) {
            context.Shell.Activate();
            _runningShellTable.Add(context.Settings);
        }

        ShellContext CreateSetupContext() {
            Logger.Debug("Creating shell context for root setup");
            return _shellContextFactory.CreateSetupContext(new ShellSettings { Name = "Default" });
        }

        ShellContext CreateShellContext(ShellSettings settings) {
            if (settings.State.CurrentState == TenantState.State.Uninitialized) {
                Logger.Debug("Creating shell context for tenant {0} setup", settings.Name);
                return _shellContextFactory.CreateSetupContext(settings);
            }

            Logger.Debug("Creating shell context for tenant {0}", settings.Name);
            return _shellContextFactory.CreateShellContext(settings);
        }

        private void SetupExtensions() {
            _extensionLoaderCoordinator.SetupExtensions();
        }

        private void MonitorExtensions() {
            // This is a "fake" cache entry to allow the extension loader coordinator
            // notify us (by resetting _current to "null") when an extension has changed
            // on disk, and we need to reload new/updated extensions.
            _cacheManager.Get("OrchardHost_Extensions",
                              ctx => {
                                  _extensionLoaderCoordinator.MonitorExtensions(ctx.Monitor);
                                  _hostLocalRestart.Monitor(ctx.Monitor);
                                  DisposeShellContext();
                                  return "";
                              });
        }

        private void DisposeShellContext() {
            if (_current != null) {
                foreach (var shellContext in _current) {
                    shellContext.Shell.Terminate();
                }
                _current = null;
            }
        }

        protected virtual void BeginRequest() {
            MonitorExtensions();
            BuildCurrent();
        }


        // the exit gate is temporary, until better control strategy is in place
        private readonly ManualResetEvent _exitGate = new ManualResetEvent(true);

        protected virtual void EndRequest() {
            if (_processingEngine.AreTasksPending()) {
                _exitGate.Reset();
                ThreadPool.QueueUserWorkItem(state => {
                    while (_processingEngine.AreTasksPending()) {
                        _processingEngine.ExecuteNextTask();
                        if (!_processingEngine.AreTasksPending()) {
                            _exitGate.Set();
                        }
                    }
                });
            }

            _exitGate.WaitOne(250);
        }

        void IShellSettingsManagerEventHandler.Saved(ShellSettings settings) {
            DisposeShellContext();
        }

        void IShellDescriptorManagerEventHandler.Changed(ShellDescriptor descriptor) {
            DisposeShellContext();
        }
    }
}
