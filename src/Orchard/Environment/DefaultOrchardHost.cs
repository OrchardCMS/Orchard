using System.Web.Mvc;
using Autofac;
using Autofac.Builder;
using Autofac.Integration.Web;
using System.Collections.Generic;
using Orchard.Mvc;

namespace Orchard.Environment {
    public class DefaultOrchardHost : IOrchardHost {
        private readonly IContainer _container;
        private readonly IContainerProvider _containerProvider;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly ControllerBuilder _controllerBuilder;

        private IOrchardRuntime _current;


        public DefaultOrchardHost(
            IContainerProvider containerProvider,
            ICompositionStrategy compositionStrategy,
            ControllerBuilder controllerBuilder) {
            _container = containerProvider.ApplicationContainer;
            _containerProvider = containerProvider;
            _compositionStrategy = compositionStrategy;
            _controllerBuilder = controllerBuilder;
        }


        public IOrchardRuntime Current {
            get { return _current; }
        }

        protected virtual void Initialize() {
            var runtime = CreateRuntime();
            runtime.Activate();
            _current = runtime;

            _controllerBuilder.SetControllerFactory(new OrchardControllerFactory());
            ServiceLocator.SetLocator(t=>_containerProvider.RequestContainer.Resolve(t));
        }

        protected virtual void EndRequest() {
            _containerProvider.DisposeRequestContainer();
        }


        protected virtual IOrchardRuntime CreateRuntime() {

            // add module types to container being built
            var addingModulesAndServices = new ContainerBuilder();
            foreach (var moduleType in _compositionStrategy.GetModuleTypes()) {
                addingModulesAndServices.Register(moduleType).As<IModule>().ContainerScoped();
            }

            // add components by the IDependency interfaces they expose
            foreach (var serviceType in _compositionStrategy.GetDependencyTypes()) {
                foreach (var interfaceType in serviceType.GetInterfaces())
                    if (typeof(IDependency).IsAssignableFrom(interfaceType))
                        addingModulesAndServices.Register(serviceType).As(interfaceType).ContainerScoped();
            }

            var runtimeContainer = _container.CreateInnerContainer();
            runtimeContainer.TagWith("runtime");
            addingModulesAndServices.Build(runtimeContainer);

            // instantiate and register modules on container being built
            var addingModules = new ContainerBuilder();
            foreach (var module in runtimeContainer.Resolve<IEnumerable<IModule>>()) {
                addingModules.RegisterModule(module);
            }
            addingModules.Build(runtimeContainer);

            return runtimeContainer.Resolve<IOrchardRuntime>();
        }

        #region IOrchardHost Members

        void IOrchardHost.Initialize() {
            Initialize();
        }
        void IOrchardHost.EndRequest() {
            EndRequest();
        }
        IOrchardRuntime IOrchardHost.CreateRuntime() {
            return CreateRuntime();
        }
        #endregion
    }
}