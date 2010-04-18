using System.Linq;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Web;
using System.Collections.Generic;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.ShellBuilders;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.ViewEngines;
using Orchard.Utility.Extensions;

namespace Orchard.Environment {
    public class DefaultOrchardHost : IOrchardHost {
        //private readonly IContainerProvider _containerProvider;
        private readonly ControllerBuilder _controllerBuilder;

        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContextFactory _shellContextFactory;

        private IEnumerable<ShellContext> _current;

        public DefaultOrchardHost(
            //IContainerProvider containerProvider,
            IShellSettingsManager shellSettingsManager,
            IShellContextFactory shellContextFactory,
            ControllerBuilder controllerBuilder) {
            //_containerProvider = containerProvider;
            _shellSettingsManager = shellSettingsManager;
            _shellContextFactory = shellContextFactory;
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
            BeginRequest();
        }

        void IOrchardHost.EndRequest() {
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
                        context.Shell.Activate();
                        return context;
                    });
            }
            
            var setupContext = CreateSetupContext();
            setupContext.Shell.Activate();
            return new[] {setupContext};
        }

        ShellContext CreateSetupContext() {
            Logger.Debug("Creating shell context for setup");
            return _shellContextFactory.Create(null);
        }

        ShellContext CreateShellContext(ShellSettings settings) {
            Logger.Debug("Creating shell context for tenant {0}", settings.Name);
            return _shellContextFactory.Create(settings);
        }

        protected virtual void BeginRequest() {
            BuildCurrent();
        }

        protected virtual void EndRequest() {
            //_containerProvider.EndRequestLifetime();
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

    }
}
