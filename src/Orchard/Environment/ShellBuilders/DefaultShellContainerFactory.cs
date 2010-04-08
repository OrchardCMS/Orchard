using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Orchard.Environment.AutofacUtil;
using Orchard.Environment.AutofacUtil.DynamicProxy2;
using Orchard.Environment.Configuration;

namespace Orchard.Environment.ShellBuilders {
    public class DefaultShellContainerFactory : IShellContainerFactory {
        private readonly IContainer _container;
        private readonly ICompositionStrategy _compositionStrategy;

        public DefaultShellContainerFactory(IContainer container, ICompositionStrategy compositionStrategy) {
            _container = container;
            _compositionStrategy = compositionStrategy;
        }

        public virtual ILifetimeScope CreateContainer(IShellSettings settings) {
            // null settings means we need to defer to the setup container factory
            if (settings == null) {
                return null;
            }

            var dynamicProxyContext = new DynamicProxyContext();

            // add module types to container being built
            var addingModulesAndServices = new ContainerUpdater();
            addingModulesAndServices.RegisterInstance(settings).As<IShellSettings>();
            addingModulesAndServices.RegisterInstance(dynamicProxyContext);
            addingModulesAndServices.RegisterType<DefaultOrchardShell>().As<IOrchardShell>().SingleInstance();

            foreach (var moduleType in _compositionStrategy.GetModuleTypes()) {
                addingModulesAndServices.RegisterType(moduleType).As<IModule>().InstancePerLifetimeScope();
            }

            // add components by the IDependency interfaces they expose
            foreach (var serviceType in _compositionStrategy.GetDependencyTypes()) {
                var registrar = addingModulesAndServices.RegisterType(serviceType)
                    .EnableDynamicProxy(dynamicProxyContext)
                    .InstancePerLifetimeScope();

                foreach (var interfaceType in serviceType.GetInterfaces()) {
                    if (typeof(IDependency).IsAssignableFrom(interfaceType)) {
                        registrar = registrar.As(interfaceType);
                        if (typeof(ISingletonDependency).IsAssignableFrom(interfaceType)) {
                            registrar = registrar.SingleInstance();
                        }
                        else if (typeof(ITransientDependency).IsAssignableFrom(interfaceType)) {
                            registrar = registrar.InstancePerDependency();
                        }
                    }
                }
            }

            var shellScope = _container.BeginLifetimeScope("shell");
            addingModulesAndServices.Update(shellScope);

            // instantiate and register modules on container being built
            var modules = shellScope.Resolve<IEnumerable<IModule>>();
            var addingModules = new ContainerUpdater();
            foreach (var module in modules) {
                addingModules.RegisterModule(module);
            }

            // DynamicProxy2.
            // addingModules.RegisterModule(new ExtensibleInterceptionModule(modules.OfType<IComponentInterceptorProvider>()));

            addingModules.Update(shellScope);

            return shellScope;
        }
    }
}
