using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orchard.Caching;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.State;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Utility.Extensions;
using Orchard.Utility;
using System.Threading;

namespace Orchard.Environment {
    // All the event handlers that DefaultOrchardHost implements have to be declared in OrchardStarter.
    public class DefaultOrchardHost : IOrchardHost, IShellSettingsManagerEventHandler, IShellDescriptorManagerEventHandler {
        private readonly IHostLocalRestart _hostLocalRestart;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly IRunningShellTable _runningShellTable;
        private readonly IProcessingEngine _processingEngine;
        private readonly IExtensionLoaderCoordinator _extensionLoaderCoordinator;
        private readonly IExtensionMonitoringCoordinator _extensionMonitoringCoordinator;
        private readonly ICacheManager _cacheManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly static object _syncLock = new object();
        private readonly static object _shellContextsWriteLock = new object();
        private readonly NamedReaderWriterLock _shellActivationLock = new NamedReaderWriterLock();

        private IEnumerable<ShellContext> _shellContexts;
        private readonly ContextState<IList<ShellSettings>> _tenantsToRestart;

        public int Retries { get; set; }
        public bool DelayRetries { get; set; }

        public DefaultOrchardHost(
            IShellSettingsManager shellSettingsManager,
            IShellContextFactory shellContextFactory,
            IRunningShellTable runningShellTable,
            IProcessingEngine processingEngine,
            IExtensionLoaderCoordinator extensionLoaderCoordinator,
            IExtensionMonitoringCoordinator extensionMonitoringCoordinator,
            ICacheManager cacheManager,
            IHostLocalRestart hostLocalRestart, 
            IHttpContextAccessor httpContextAccessor) {

            _shellSettingsManager = shellSettingsManager;
            _shellContextFactory = shellContextFactory;
            _runningShellTable = runningShellTable;
            _processingEngine = processingEngine;
            _extensionLoaderCoordinator = extensionLoaderCoordinator;
            _extensionMonitoringCoordinator = extensionMonitoringCoordinator;
            _cacheManager = cacheManager;
            _hostLocalRestart = hostLocalRestart;
            _httpContextAccessor = httpContextAccessor;

            _tenantsToRestart = new ContextState<IList<ShellSettings>>("DefaultOrchardHost.TenantsToRestart", () => new List<ShellSettings>());

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public IList<ShellContext> Current {
            get { return BuildCurrent().ToReadOnlyCollection(); }
        }

        public ShellContext GetShellContext(ShellSettings shellSettings) {
            return BuildCurrent().SingleOrDefault(shellContext => shellContext.Settings.Name.Equals(shellSettings.Name));
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
            var workContext = shellContext.LifetimeScope.CreateWorkContextScope();
            return new StandaloneEnvironmentWorkContextScopeWrapper(workContext, shellContext);
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
            while (_tenantsToRestart.GetState().Any()) {
                var settings = _tenantsToRestart.GetState().First();
                _tenantsToRestart.GetState().Remove(settings);
                Logger.Debug("Updating shell: " + settings.Name);
                lock (_syncLock) {
                    ActivateShell(settings);
                }
            }
        }

        void CreateAndActivateShells() {
            Logger.Information("Start creation of shells");

            // Is there any tenant right now?
            var allSettings = _shellSettingsManager.LoadSettings()
                .Where(settings => settings.State == TenantState.Running || settings.State == TenantState.Uninitialized || settings.State == TenantState.Initializing)
                .ToArray();

            // Load all tenants, and activate their shell.
            if (allSettings.Any()) {
                Parallel.ForEach(allSettings, settings => {
                    for (var i = 0; i <= Retries; i++) {

                        // Not the first attempt, wait for a while ...
                        if (DelayRetries && i > 0) {

                            // Wait for i^2 which means 1, 2, 4, 8 ... seconds
                            Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(i, 2)));
                        }

                        try {
                            var context = CreateShellContext(settings);
                            ActivateShell(context);

                            // If everything went well, return to stop the retry loop
                            break;
                        }
                        catch (Exception ex) {
                            if (i == Retries) {
                                Logger.Fatal("A tenant could not be started: {0} after {1} retries.", settings.Name, Retries);
                                return;
                            }
                            else {
                                Logger.Error(ex, "A tenant could not be started: " + settings.Name + " Attempt number: " + i);
                            }
                        }
                        
                    }

                    while (_processingEngine.AreTasksPending()) {
                        Logger.Debug("Processing pending task after activate Shell");
                        _processingEngine.ExecuteNextTask();
                    }
                });
            }
            // No settings, run the Setup.
            else {
                var setupContext = CreateSetupContext();
                ActivateShell(setupContext);
            }

            Logger.Information("Done creating shells");
        }

        /// <summary>
        /// Starts a Shell and registers its settings in RunningShellTable
        /// </summary>
        private void ActivateShell(ShellContext context) {
            Logger.Debug("Activating context for tenant {0}", context.Settings.Name);
            context.Shell.Activate();

            lock (_shellContextsWriteLock) {
                _shellContexts = (_shellContexts ?? Enumerable.Empty<ShellContext>())
                                .Where(c => c.Settings.Name != context.Settings.Name)
                                .Concat(new[] { context })
                                .ToArray();
            }

            _runningShellTable.Add(context.Settings);
        }

        /// <summary>
        /// Creates a transient shell for the default tenant's setup.
        /// </summary>
        private ShellContext CreateSetupContext() {
            Logger.Debug("Creating shell context for root setup.");
            return _shellContextFactory.CreateSetupContext(new ShellSettings { Name = ShellSettings.DefaultName });
        }

        /// <summary>
        /// Creates a shell context based on shell settings.
        /// </summary>
        private ShellContext CreateShellContext(ShellSettings settings) {
            if (settings.State == TenantState.Uninitialized || settings.State == TenantState.Invalid) {
                Logger.Debug("Creating shell context for tenant {0} setup.", settings.Name);
                return _shellContextFactory.CreateSetupContext(settings);
            }

            Logger.Debug("Creating shell context for tenant {0}.", settings.Name);
            return _shellContextFactory.CreateShellContext(settings);
        }

        private void SetupExtensions() {
            _extensionLoaderCoordinator.SetupExtensions();
        }

        private void MonitorExtensions() {
            // This is a "fake" cache entry to allow the extension loader coordinator
            // notify us (by resetting _current to "null") when an extension has changed
            // on disk, and we need to reload new/updated extensions.
            _cacheManager.Get("OrchardHost_Extensions", true,
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
                lock (_syncLock) {
                    if (_shellContexts != null) {
                        foreach (var shellContext in _shellContexts) {
                            shellContext.Shell.Terminate();
                            shellContext.Dispose();
                        }
                    }
                }
                _shellContexts = null;
            }
        }

        protected virtual void BeginRequest() {
            BlockRequestsDuringSetup();

            Action ensureInitialized = () => {
                // Ensure all shell contexts are loaded, or need to be reloaded if
                // extensions have changed
                MonitorExtensions();
                BuildCurrent();
            };

            ShellSettings currentShellSettings = null;

            var httpContext = _httpContextAccessor.Current();
            if (httpContext != null) {
                currentShellSettings = _runningShellTable.Match(httpContext);
            }

            if (currentShellSettings == null) {
                ensureInitialized();
            }
            else {
                _shellActivationLock.RunWithReadLock(currentShellSettings.Name, () => {
                    ensureInitialized();
                });
            }

            // StartUpdatedShells can cause a writer shell activation lock so it should run outside the reader lock.
            StartUpdatedShells();
        }

        protected virtual void EndRequest() {
            // Synchronously process all pending tasks. It's safe to do this at this point
            // of the pipeline, as the request transaction has been closed, so creating a new
            // environment and transaction for these tasks will behave as expected.)
            while (_processingEngine.AreTasksPending()) {
                Logger.Debug("Processing pending task");
                _processingEngine.ExecuteNextTask();
            }

            StartUpdatedShells();
        }

        void IShellSettingsManagerEventHandler.Saved(ShellSettings settings) {
            Logger.Debug("Shell saved: " + settings.Name);

            // if a tenant has been created
            if (settings.State != TenantState.Invalid) {
                if (!_tenantsToRestart.GetState().Any(t => t.Name.Equals(settings.Name))) {
                    Logger.Debug("Adding tenant to restart: " + settings.Name + " " + settings.State);
                    _tenantsToRestart.GetState().Add(settings);
                }
            }
        }

        public void ActivateShell(ShellSettings settings) {
            Logger.Debug("Activating shell: " + settings.Name);

            // look for the associated shell context
            var shellContext = _shellContexts.FirstOrDefault(c => c.Settings.Name == settings.Name);

            if (shellContext == null && settings.State == TenantState.Disabled) {
                return;
            }

            // is this is a new tenant ? or is it a tenant waiting for setup ?
            if (shellContext == null || settings.State == TenantState.Uninitialized) {
                // create the Shell
                var context = CreateShellContext(settings);

                // activate the Shell
                ActivateShell(context);
            }
            // terminate the shell if the tenant was disabled
            else if (settings.State == TenantState.Disabled) {
                shellContext.Shell.Terminate();
                _runningShellTable.Remove(settings);

                // Forcing enumeration with ToArray() so a lazy execution isn't causing issues by accessing the disposed context.
                _shellContexts = _shellContexts.Where(shell => shell.Settings.Name != settings.Name).ToArray();

                shellContext.Dispose();
            }
            // reload the shell as its settings have changed
            else {
                _shellActivationLock.RunWithWriteLock(settings.Name, () => {
                    // dispose previous context
                    shellContext.Shell.Terminate();

                    var context = _shellContextFactory.CreateShellContext(settings);

                    // Activate and register modified context.
                    // Forcing enumeration with ToArray() so a lazy execution isn't causing issues by accessing the disposed shell context.
                    _shellContexts = _shellContexts.Where(shell => shell.Settings.Name != settings.Name).Union(new[] { context }).ToArray();

                    shellContext.Dispose();
                    context.Shell.Activate();

                    _runningShellTable.Update(settings);
                });
            }
        }

        /// <summary>
        /// A feature is enabled/disabled, the tenant needs to be restarted
        /// </summary>
        void IShellDescriptorManagerEventHandler.Changed(ShellDescriptor descriptor, string tenant) {
            if (_shellContexts == null) {
                return;
            }

            Logger.Debug("Shell changed: " + tenant);

            var context = _shellContexts.FirstOrDefault(x => x.Settings.Name == tenant);

            if (context == null) {
                return;
            }

            // don't restart when tenant is in setup
            if (context.Settings.State != TenantState.Running) {
                return;
            }

            // don't flag the tenant if already listed
            if (_tenantsToRestart.GetState().Any(x => x.Name == tenant)) {
                return;
            }

            Logger.Debug("Adding tenant to restart: " + tenant);
            _tenantsToRestart.GetState().Add(context.Settings);
        }

        private void BlockRequestsDuringSetup() {
            var httpContext = _httpContextAccessor.Current();
            if (httpContext.IsBackgroundContext())
                return;

            // Get the requested shell.
            var runningShell = _runningShellTable.Match(httpContext);
            if (runningShell == null)
                return;

            // If the requested shell is currently initializing, return a Service Unavailable HTTP status code.
            if (runningShell.State == TenantState.Initializing) {
                var response = httpContext.Response;
                response.StatusCode = 503;
                response.StatusDescription = "This tenant is currently initializing. Please try again later.";
                response.Write("This tenant is currently initializing. Please try again later.");
            }
        }

        // To be used from CreateStandaloneEnvironment(), also disposes the ShellContext LifetimeScope.
        private class StandaloneEnvironmentWorkContextScopeWrapper : IWorkContextScope {
            private readonly ShellContext _shellContext;
            private readonly IWorkContextScope _workContextScope;

            public WorkContext WorkContext {
                get { return _workContextScope.WorkContext; }
            }

            public StandaloneEnvironmentWorkContextScopeWrapper(IWorkContextScope workContextScope, ShellContext shellContext) {
                _workContextScope = workContextScope;
                _shellContext = shellContext;
            }

            public TService Resolve<TService>() {
                return _workContextScope.Resolve<TService>();
            }

            public bool TryResolve<TService>(out TService service) {
                return _workContextScope.TryResolve<TService>(out service);
            }

            public void Dispose() {
                _workContextScope.Dispose();
                _shellContext.Dispose();
            }
        }
    }
}
