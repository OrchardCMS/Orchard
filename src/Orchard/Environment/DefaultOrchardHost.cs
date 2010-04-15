using System;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Web;
using System.Collections.Generic;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;
using Orchard.Extensions;
using Orchard.Mvc;
using Orchard.Mvc.ViewEngines;
using Orchard.Utility.Extensions;

namespace Orchard.Environment {
    public class DefaultOrchardHost : IOrchardHost {
        private readonly IContainerProvider _containerProvider;
        private readonly ControllerBuilder _controllerBuilder;

        private readonly ITenantManager _tenantManager;
        private readonly IShellContextFactory _shellContextFactory;

        private IEnumerable<ShellContext> _current;

        public DefaultOrchardHost(
            IContainerProvider containerProvider,
            ITenantManager tenantManager,
            IShellContextFactory shellContextFactory,
            ControllerBuilder controllerBuilder) {
            _containerProvider = containerProvider;
            _tenantManager = tenantManager;
            _shellContextFactory = shellContextFactory;
            _controllerBuilder = controllerBuilder;
        }

        public IList<ShellContext> Current {
            get { return BuildCurrent().ToReadOnlyCollection(); }
        }

        void IOrchardHost.Initialize() {
            ViewEngines.Engines.Insert(0, LayoutViewEngine.CreateShim());
            _controllerBuilder.SetControllerFactory(new OrchardControllerFactory());
            ServiceLocator.SetLocator(t => _containerProvider.RequestLifetime.Resolve(t));

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
            var shellContext = _shellContextFactory.Create(shellSettings);
            return new StandaloneEnvironment(shellContext.LifetimeScope);
        }

        IEnumerable<ShellContext> GetCurrent() {
            lock (this) {
                return _current ?? (_current = BuildCurrent());
            }
        }

        IEnumerable<ShellContext> BuildCurrent() {
            return CreateAndActivate().ToArray();
        }

        IEnumerable<ShellContext> CreateAndActivate() {
            foreach(var settings in _tenantManager.LoadSettings()) {
                var context = _shellContextFactory.Create(settings);
                context.Shell.Activate();
                yield return context;
            }
        }

        protected virtual void BeginRequest() {
            BuildCurrent();
        }

        protected virtual void EndRequest() {
            _containerProvider.EndRequestLifetime();
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
