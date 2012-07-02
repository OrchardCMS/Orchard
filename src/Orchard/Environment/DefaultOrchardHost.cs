using System;
using System.Linq;
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
        private readonly IExtensionMonitoringCoordinator _extensionMonitoringCoordinator;
        private readonly ICacheManager _cacheManager;
        private readonly object _syncLock = new object();

        private IEnumerable<ShellContext> _shellContexts;
        private IEnumerable<ShellSettings> _tenantsToRestart; 

        public DefaultOrchardHost(
            IShellSettingsManager shellSettingsManager,
            IShellContextFactory shellContextFactory,
            IRunningShellTable runningShellTable,
            IProcessingEngine processingEngine,
            IExtensionLoaderCoordinator extensionLoaderCoordinator,
            IExtensionMonitoringCoordinator extensionMonitoringCoordinator,
            ICacheManager cacheManager,
            IHostLocalRestart hostLocalRestart ) {
            _shellSettingsManager = shellSettingsManager;
            _shellContextFactory = shellContextFactory;
            _runningShellTable = runningShellTable;
            _processingEngine = processingEngine;
            _extensionLoaderCoordinator = extensionLoaderCoordinator;
            _extensionMonitoringCoordinator = extensionMonitoringCoordinator;
            _cacheManager = cacheManager;
            _hostLocalRestart = hostLocalRestart;
            _tenantsToRestart = Enumerable.Empty<ShellSettings>();

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public IList<ShellContext> Current {
            get { return BuildCurrent().ToReadOnlyCollection(); }
        }

        public ShellContext GetShellContext(ShellSettings shellSettings) {
            return Current
                .Single(shellContext => shellContext.Settings.Name.Equals(shellSettings.Name));
        }

        void IOrchardHost.Initialize() {
            Logger.Information("Initializing");
            BuildCurrent();
            Logger.Information("Initialized");
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

        /// <summary>
        /// Ensures shells are activated, or re-activated if extensions have changed
        /// </summary>
        IEnumerable<ShellContext> BuildCurrent() {
            if (_shellContexts == null) {
                lock (_syncLock) {
                    if (_shellContexts == null) {
                        SetupExtensions();
                        MonitorExtensions();
                        CreateAndActivateShells();
                    }
                }
            }

            return _shellContexts;
        }

        void StartUpdatedShells() {
            lock (_syncLock) {
                if (_tenantsToRestart.Any()) {
                    foreach (var settings in _tenantsToRestart.Distinct().ToList()) {
                        ActivateShell(settings);
                    }

                    _tenantsToRestart = Enumerable.Empty<ShellSettings>();
                }
            }
        }

        void CreateAndActivateShells() {
            Logger.Information("Start creation of shells");

            // is there any tenant right now ?
            var allSettings = _shellSettingsManager.LoadSettings().ToArray();

            // load all tenants, and activate their shell
            if (allSettings.Any()) {
                foreach (var settings in allSettings) {
                    try {
                        var context = CreateShellContext(settings);
                        ActivateShell(context);
                    }
                    catch(Exception e) {
                        Logger.Error(e, "A tenant could not be started: " + settings.Name);
                    }
                }
            }
            // no settings, run the Setup
            else {
                var setupContext = CreateSetupContext();
                ActivateShell(setupContext);
            }

            Logger.Information("Done creating shells");
        }

        /// <summary>
        /// Start a Shell and register its settings in RunningShellTable
        /// </summary>
        private void ActivateShell(ShellContext context) {
            Logger.Debug("Activating context for tenant {0}", context.Settings.Name); 
            context.Shell.Activate();

            _shellContexts = (_shellContexts ?? Enumerable.Empty<ShellContext>()).Union(new [] {context});
            _runningShellTable.Add(context.Settings);
        }

        ShellContext CreateSetupContext() {
            Logger.Debug("Creating shell context for root setup");
            return _shellContextFactory.CreateSetupContext(new ShellSettings { Name = ShellSettings.DefaultName });
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
                                  _extensionMonitoringCoordinator.MonitorExtensions(ctx.Monitor);
                                  _hostLocalRestart.Monitor(ctx.Monitor);
                                  DisposeShellContext();
                                  return "";
                              });
        }

        /// <summary>
        /// Terminates all active shell contexts, and dispose their scope, forcing
        /// them to be reloaded if necessary.
        /// </summary>
        private void DisposeShellContext() {
            Logger.Information("Disposing active shell contexts");

            if (_shellContexts != null) {
                foreach (var shellContext in _shellContexts) {
                    shellContext.Shell.Terminate();
                    shellContext.LifetimeScope.Dispose();
                }
                _shellContexts = null;
            }
        }

        protected virtual void BeginRequest() {
            // Ensure all shell contexts are loaded, or need to be reloaded if
            // extensions have changed
            MonitorExtensions();
            BuildCurrent();
            StartUpdatedShells();
        }

        protected virtual void EndRequest() {
            // Synchronously process all pending tasks. It's safe to do this at this point
            // of the pipeline, as the request transaction has been closed, so creating a new
            // environment and transaction for these tasks will behave as expected.)
            while (_processingEngine.AreTasksPending()) {
                _processingEngine.ExecuteNextTask();
            }
        }

        /// <summary>
        /// Register and activate a new Shell when a tenant is created
        /// </summary>
        void IShellSettingsManagerEventHandler.Saved(ShellSettings settings) {
            lock (_syncLock) {
                
                // if a tenant has been altered, and is not invalid, reload it
                if (settings.State.CurrentState != TenantState.State.Invalid) {
                    _tenantsToRestart = _tenantsToRestart.Where(x => x.Name != settings.Name).Union(new[] { settings });
                }
            }
        }

        void ActivateShell(ShellSettings settings) {
            // look for the associated shell context
            var shellContext = _shellContexts.FirstOrDefault(c => c.Settings.Name == settings.Name);

            // is this is a new tenant ? or is it a tenant waiting for setup ?
            if (shellContext == null || settings.State.CurrentState == TenantState.State.Uninitialized) {
                // create the Shell
                var context = CreateShellContext(settings);

                // activate the Shell
                ActivateShell(context);
            }
            // reload the shell as its settings have changed
            else {
                // dispose previous context
                shellContext.Shell.Terminate();
                shellContext.LifetimeScope.Dispose();

                var context = _shellContextFactory.CreateShellContext(settings);

                // activate and register modified context
                _shellContexts = _shellContexts.Where(shell => shell.Settings.Name != settings.Name).Union(new[] { context });

                context.Shell.Activate();

                _runningShellTable.Update(settings);
            }
        }

        /// <summary>
        /// A feature is enabled/disabled
        /// </summary>
        void IShellDescriptorManagerEventHandler.Changed(ShellDescriptor descriptor, string tenant) {
            lock (_syncLock) {
                
                if (_shellContexts == null) {
                    return;
                }

                var context =_shellContexts.FirstOrDefault(x => x.Settings.Name == tenant);
                
                // some shells might need to be started, e.g. created by command line
                if(context == null) {
                    StartUpdatedShells();
                    context = _shellContexts.First(x => x.Settings.Name == tenant);
                }

                // don't update the settings themselves here
                if(_tenantsToRestart.Any(x => x.Name == tenant)) {
                    return;
                }

                _tenantsToRestart = _tenantsToRestart.Union(new[] { context.Settings });
            }
        }
    }
}
