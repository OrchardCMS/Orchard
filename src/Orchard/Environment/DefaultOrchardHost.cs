using System;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using System.Collections.Generic;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.ShellBuilders;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.ViewEngines;
using Orchard.Utility.Extensions;
using Orchard.Events;

namespace Orchard.Environment {
    public class DefaultOrchardHost : IOrchardHost {
        private readonly ControllerBuilder _controllerBuilder;

        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly IRunningShellTable _runningShellTable;

        private IEnumerable<ShellContext> _current;

        public DefaultOrchardHost(
            IShellSettingsManager shellSettingsManager,
            IShellContextFactory shellContextFactory,
            IRunningShellTable runningShellTable,
            ControllerBuilder controllerBuilder) {
            //_containerProvider = containerProvider;
            _shellSettingsManager = shellSettingsManager;
            _shellContextFactory = shellContextFactory;
            _runningShellTable = runningShellTable;
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

        void IOrchardHost.Reinitialize_Obsolete() {
            _current = null;
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
            HackSimulateExtensionActivation(context.LifetimeScope);
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

        protected virtual void EndRequest() {
        }


        private void HackSimulateExtensionActivation(ILifetimeScope shellContainer) {
            var containerProvider = new FiniteContainerProvider(shellContainer);
            try {
                var requestContainer = containerProvider.RequestLifetime;

                var hackInstallationGenerator = requestContainer.Resolve<IHackInstallationGenerator>();
                hackInstallationGenerator.GenerateActivateEvents();
            }
            finally {
                containerProvider.EndRequestLifetime();
            }
        }

        public void Process(string messageName, IDictionary<string, string> eventData) {
            if (messageName == "ShellDescriptor_Changed") {
                _current = null;
            }
        }
        public void InvalidateShells() {
            _current = null;
        }
    }
}
