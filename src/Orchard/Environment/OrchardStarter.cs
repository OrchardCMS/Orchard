using System;
using Autofac;
using Autofac.Builder;
using Autofac.Integration.Web;
using Autofac.Modules;

namespace Orchard.Environment {
    public static class OrchardStarter {
        public static IContainer CreateHostContainer(Action<ContainerBuilder> registrations) {
            var builder = new ContainerBuilder();

            // Modifies the container to automatically return IEnumerable<T> of service type T
            builder.RegisterModule(new ImplicitCollectionSupportModule());

            // a single default host implementation is needed for bootstrapping a web app domain
            builder.Register<DefaultOrchardHost>().As<IOrchardHost>()
                .SingletonScoped();
            builder.Register<DefaultCompositionStrategy>().As<ICompositionStrategy>()
                .SingletonScoped();
            builder.Register<DefaultOrchardRuntime>().As<IOrchardRuntime>()
                .ContainerScoped().InContext("runtime");

            // The container provider gives you access to the lowest container at the time, 
            // and dynamically creates a per-request container. The DisposeRequestContainer method
            // still needs to be called on end request, but that's the host component's job to worry about.
            builder.Register<ContainerProvider>().As<IContainerProvider>().ContainerScoped();

            registrations(builder);

            return builder.Build();
        }

        public static IOrchardHost CreateHost(Action<ContainerBuilder> registrations) {
            return CreateHostContainer(registrations).Resolve<IOrchardHost>();
        }
    }
}
