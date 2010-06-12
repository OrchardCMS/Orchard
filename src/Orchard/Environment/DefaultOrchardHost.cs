using System;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Autofac;
using System.Collections.Generic;
using Autofac.Features.OwnedInstances;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.State;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.ViewEngines;
using Orchard.Utility.Extensions;

namespace Orchard.Environment {
    public class DefaultOrchardHost : IOrchardHost, IShellSettingsManagerEventHandler, IShellDescriptorManagerEventHandler {
        private readonly ControllerBuilder _controllerBuilder;

        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly IRunningShellTable _runningShellTable;
        private readonly IProcessingEngine _processingEngine;

        private IEnumerable<ShellContext> _current;

        public DefaultOrchardHost(
            IShellSettingsManager shellSettingsManager,
            IShellContextFactory shellContextFactory,
            IRunningShellTable runningShellTable,
            IProcessingEngine processingEngine,
            ControllerBuilder controllerBuilder) {
            //_containerProvider = containerProvider;
            _shellSettingsManager = shellSettingsManager;
            _shellContextFactory = shellContextFactory;
            _runningShellTable = runningShellTable;
            _processingEngine = processingEngine;
            _controllerBuilder = controllerBuilder;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public IList<ShellContext> Current {
            get { return BuildCurrent().ToReadOnlyCollection(); }
        }

        void IOrchardHost.Initialize() {
            Logger.Information("Initializing");
            ViewEngines.Engines.Insert(0, LayoutViewEngine.CreateShim());
            _controllerBuilder.SetControllerFactory(new OrchardControllerFactory());
            //ServiceLocator.SetLocator(t => _containerProvider.RequestLifetime.Resolve(t));

            BuildCurrent();
        }


        void IOrchardHost.BeginRequest() {
            Logger.Debug("BeginRequest");
            BeginRequest();
        }

        void IOrchardHost.EndRequest() {
            Logger.Debug("EndRequest");
            EndRequest();
        }

        IStandaloneEnvironment IOrchardHost.CreateStandaloneEnvironment(ShellSettings shellSettings) {
            Logger.Debug("Creating standalone environment for tenant {0}", shellSettings.Name);
            var shellContext = CreateShellContext(shellSettings);
            return new StandaloneEnvironment(shellContext.LifetimeScope);
        }

        IEnumerable<ShellContext> BuildCurrent() {
            lock (this) {
                return _current ?? (_current = CreateAndActivate().ToArray());
            }
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

        protected virtual void BeginRequest() {
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
            _current = null;
        }

        void IShellDescriptorManagerEventHandler.Changed(ShellDescriptor descriptor) {
            _current = null;
        }
    }
}
