using System.Linq;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Web;
using System.Collections.Generic;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders;
using Orchard.Extensions;
using Orchard.Mvc;
using Orchard.Mvc.ViewEngines;

namespace Orchard.Environment {
    public class DefaultOrchardHost : IOrchardHost {
        private readonly IContainerProvider _containerProvider;
        private readonly ControllerBuilder _controllerBuilder;
        private readonly IEnumerable<IShellContainerFactory_Obsolete> _shellContainerFactories;

        private readonly ITenantManager _tenantManager;
        private IOrchardShell _current;


        public DefaultOrchardHost(
            IContainerProvider containerProvider,
            ITenantManager tenantManager,
            ControllerBuilder controllerBuilder,
            IEnumerable<IShellContainerFactory_Obsolete> shellContainerFactories) {
            _containerProvider = containerProvider;
            _tenantManager = tenantManager;
            _controllerBuilder = controllerBuilder;
            _shellContainerFactories = shellContainerFactories;
        }


        public IOrchardShell Current {
            get { return _current; }
        }


        void IOrchardHost.Initialize() {
            ViewEngines.Engines.Insert(0, LayoutViewEngine.CreateShim());
            _controllerBuilder.SetControllerFactory(new OrchardControllerFactory());
            ServiceLocator.SetLocator(t => _containerProvider.RequestLifetime.Resolve(t));

            CreateAndActivateShell();
        }

        void IOrchardHost.Reinitialize() {
            _current = null;
            //CreateAndActivateShell();
        }


        void IOrchardHost.BeginRequest() {
            BeginRequest();
        }

        void IOrchardHost.EndRequest() {
            EndRequest();
        }

        IStandaloneEnvironment IOrchardHost.CreateStandaloneEnvironment(ShellSettings shellSettings) {
            var shellContainer = CreateShellContainer(shellSettings);
            return new StandaloneEnvironment(shellContainer);
        }

        protected virtual void CreateAndActivateShell() {
            lock (this) {
                if (_current != null) {
                    return;
                }

                var shellContainer = CreateShellContainer();
                var shell = shellContainer.Resolve<IOrchardShell>();
                shell.Activate();
                _current = shell;

                // Activate extensions inside shell container
                HackSimulateExtensionActivation(shellContainer);
            }
        }

        protected virtual void BeginRequest() {
            if (_current == null) {
                CreateAndActivateShell();
            }
        }

        protected virtual void EndRequest() {
            _containerProvider.EndRequestLifetime();
        }


        public virtual IOrchardShell CreateShell() {
            return CreateShellContainer().Resolve<IOrchardShell>();
        }

        public virtual ILifetimeScope CreateShellContainer() {
            var settings = _tenantManager.LoadSettings();
            if (settings.Any()) {
                //TEMP: multi-tenancy not implemented yet
                var shellContainer = CreateShellContainer(settings.Single());
                if (shellContainer != null)
                    return shellContainer;
            }
            return CreateShellContainer(null);
        }

        private ILifetimeScope CreateShellContainer(ShellSettings shellSettings) {
            foreach (var factory in _shellContainerFactories) {
                var container = factory.CreateContainer(shellSettings);
                if (container != null) {
                    return container;
                }
            }
            return null;
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