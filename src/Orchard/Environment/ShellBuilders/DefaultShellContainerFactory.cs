using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Builder;
using AutofacContrib.DynamicProxy2;
using Orchard.Environment.Configuration;

namespace Orchard.Environment.ShellBuilders {
    public class DefaultShellContainerFactory : IShellContainerFactory {
        private readonly IContainer _container;
        private readonly ICompositionStrategy _compositionStrategy;

        public DefaultShellContainerFactory(IContainer container, ICompositionStrategy compositionStrategy) {
            _container = container;
            _compositionStrategy = compositionStrategy;
        }

        public virtual IContainer CreateContainer(IShellSettings settings) {
            // null settings means we need to defer to the setup container factory
            if (settings == null) {
                return null;
            }

            // add module types to container being built
            var addingModulesAndServices = new ContainerBuilder();
            addingModulesAndServices.Register<DefaultOrchardShell>().As<IOrchardShell>().SingletonScoped();

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

            return shellContainer;
        }
    }
}
