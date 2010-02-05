using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using Autofac.Builder;
using Autofac.Integration.Web;
using System.Collections.Generic;
using AutofacContrib.DynamicProxy2;
using Orchard.Environment.ShellBuilders;
using Orchard.Extensions;
using Orchard.Mvc;
using Orchard.Mvc.ViewEngines;
using Orchard.Tasks;

namespace Orchard.Environment {
    public class DefaultOrchardHost : IOrchardHost {
        private readonly IContainerProvider _containerProvider;
        private readonly ControllerBuilder _controllerBuilder;
        private readonly IEnumerable<IShellContainerFactory> _shellContainerFactories;

        private IOrchardShell _current;


        public DefaultOrchardHost(
            IContainerProvider containerProvider,
            ControllerBuilder controllerBuilder,
            IEnumerable<IShellContainerFactory> shellContainerFactories) {
            _containerProvider = containerProvider;
            _controllerBuilder = controllerBuilder;
            _shellContainerFactories = shellContainerFactories;
        }


        public IOrchardShell Current {
            get { return _current; }
        }

        void IOrchardHost.Initialize() {
            Initialize();
        }
        void IOrchardHost.EndRequest() {
            EndRequest();
        }

        protected virtual void Initialize() {
            var shellContainer = CreateShellContainer();
            var shell = shellContainer.Resolve<IOrchardShell>();
            shell.Activate();
            _current = shell;

            ViewEngines.Engines.Insert(0, LayoutViewEngine.CreateShim());
            _controllerBuilder.SetControllerFactory(new OrchardControllerFactory());
            ServiceLocator.SetLocator(t => _containerProvider.RequestContainer.Resolve(t));

            // Fire off one-time install events on an alternate container
            HackInstallSimulation();

            // Activate extensions inside shell container
            HackSimulateExtensionActivation(shellContainer);
        }


        protected virtual void EndRequest() {
            _containerProvider.DisposeRequestContainer();
        }


        public virtual IOrchardShell CreateShell() {
            return CreateShellContainer().Resolve<IOrchardShell>();
        }

        public virtual IContainer CreateShellContainer() {
            foreach(var factory in _shellContainerFactories) {
                var container = factory.CreateContainer(null);
                if (container != null)
                    return container;
            }
            return null;
        }

        private void HackInstallSimulation() {
            var tempContainer = CreateShellContainer();
            var containerProvider = new FiniteContainerProvider(tempContainer);
            try {
                var requestContainer = containerProvider.RequestContainer;

                var hackInstallationGenerator = requestContainer.Resolve<IHackInstallationGenerator>();
                hackInstallationGenerator.GenerateInstallEvents();
            }
            finally {
                // shut everything down again
                containerProvider.DisposeRequestContainer();
                tempContainer.Dispose();
            }
        }

        private void HackSimulateExtensionActivation(IContainer shellContainer) {
            var containerProvider = new FiniteContainerProvider(shellContainer);
            try {
                var requestContainer = containerProvider.RequestContainer;

                var hackInstallationGenerator = requestContainer.Resolve<IHackInstallationGenerator>();
                hackInstallationGenerator.GenerateActivateEvents();
            }
            finally {
                containerProvider.DisposeRequestContainer();
            }
        }

    }
}