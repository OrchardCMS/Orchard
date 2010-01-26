using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using Autofac.Builder;
using Autofac.Integration.Web;
using System.Collections.Generic;
using AutofacContrib.DynamicProxy2;
using Orchard.Extensions;
using Orchard.Mvc;
using Orchard.Mvc.ViewEngines;
using Orchard.Tasks;

namespace Orchard.Environment {
    public class DefaultOrchardHost : IOrchardHost {
        private readonly IContainer _container;
        private readonly IContainerProvider _containerProvider;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly ControllerBuilder _controllerBuilder;

        private IOrchardShell _current;


        public DefaultOrchardHost(
            IContainerProvider containerProvider,
            ICompositionStrategy compositionStrategy,
            ControllerBuilder controllerBuilder) {
            _container = containerProvider.ApplicationContainer;
            _containerProvider = containerProvider;
            _compositionStrategy = compositionStrategy;
            _controllerBuilder = controllerBuilder;
        }


        public IOrchardShell Current {
            get { return _current; }
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

        protected virtual void EndRequest() {
            _containerProvider.DisposeRequestContainer();
        }


        protected virtual IOrchardShell CreateShell() {
            var shellContainer = CreateShellContainer();

            return shellContainer.Resolve<IOrchardShell>();
        }

        public virtual IContainer CreateShellContainer() {
            // add module types to container being built
            var addingModulesAndServices = new ContainerBuilder();
            foreach (var moduleType in _compositionStrategy.GetModuleTypes()) {
                addingModulesAndServices.Register(moduleType).As<IModule>().ContainerScoped();
            }

            // add components by the IDependency interfaces they expose
            foreach (var serviceType in _compositionStrategy.GetDependencyTypes()) {
                foreach (var interfaceType in serviceType.GetInterfaces()) {
                    if (typeof(IDependency).IsAssignableFrom(interfaceType)) {
                        var registrar = addingModulesAndServices.Register(serviceType).As(interfaceType);
                        if (typeof(ISingletonDependency).IsAssignableFrom(interfaceType)) {
                            registrar.SingletonScoped();
                        }
                        else if (typeof(ITransientDependency).IsAssignableFrom(interfaceType)) {
                            registrar.FactoryScoped();
                        }
                        else {
                            registrar.ContainerScoped();
                        }
                    }
                }
            }

            var shellContainer = _container.CreateInnerContainer();
            shellContainer.TagWith("shell");
            addingModulesAndServices.Build(shellContainer);

            // instantiate and register modules on container being built
            var modules = shellContainer.Resolve<IEnumerable<IModule>>();
            var addingModules = new ContainerBuilder();
            foreach (var module in modules) {
                addingModules.RegisterModule(module);
            }
            addingModules.RegisterModule(new ExtensibleInterceptionModule(modules.OfType<IComponentInterceptorProvider>()));
            addingModules.Build(shellContainer);



            //foreach (var reg in shellContainer.ComponentRegistrations) {
            //    reg.Preparing += (s, e) => {
            //        Debug.WriteLine(e.Component.Descriptor.BestKnownImplementationType.FullName);
            //    };
            //}

            return shellContainer;
        }

        #region IOrchardHost Members

        void IOrchardHost.Initialize() {
            Initialize();
        }
        void IOrchardHost.EndRequest() {
            EndRequest();
        }
        IOrchardShell IOrchardHost.CreateShell() {
            return CreateShell();
        }
        #endregion
    }

    public class ExtensibleInterceptionModule : InterceptionModule {
        public ExtensibleInterceptionModule(IEnumerable<IComponentInterceptorProvider> providers)
            : base(new CombinedProvider(providers.Concat(new[] { new FlexibleInterceptorProvider() })), new FlexibleInterceptorAttacher()) {
        }

        class CombinedProvider : IComponentInterceptorProvider {
            private readonly IEnumerable<IComponentInterceptorProvider> _providers;

            public CombinedProvider(IEnumerable<IComponentInterceptorProvider> providers) {
                _providers = providers;
            }

            public IEnumerable<Service> GetInterceptorServices(IComponentDescriptor descriptor) {
                return _providers
                    .SelectMany(x => x.GetInterceptorServices(descriptor))
                    .Distinct()
                    .ToList();
            }
        }
    }
}