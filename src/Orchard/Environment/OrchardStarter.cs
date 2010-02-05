using System;
using System.Diagnostics;
using Autofac;
using Autofac.Builder;
using Autofac.Integration.Web;
using Autofac.Modules;
using AutofacContrib.DynamicProxy2;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders;
using Orchard.Extensions;
using Orchard.Extensions.Loaders;

namespace Orchard.Environment {
    public static class OrchardStarter {
        public static IContainer CreateHostContainer(Action<ContainerBuilder> registrations) {
            var builder = new ContainerBuilder();

            // Modifies the container to automatically return IEnumerable<T> of service type T
            builder.RegisterModule(new ImplicitCollectionSupportModule());

            // a single default host implementation is needed for bootstrapping a web app domain
            builder.Register<DefaultOrchardHost>().As<IOrchardHost>().SingletonScoped();
            builder.Register<DefaultCompositionStrategy>().As<ICompositionStrategy>().SingletonScoped();
            builder.Register<DefaultShellContainerFactory>().As<IShellContainerFactory>().SingletonScoped();
            builder.Register<ShellSettingsLoader>().As<IShellSettingsLoader>().SingletonScoped();
            builder.Register<SetupShellContainerFactory>().As<IShellContainerFactory>().SingletonScoped();

            // The container provider gives you access to the lowest container at the time, 
            // and dynamically creates a per-request container. The DisposeRequestContainer method
            // still needs to be called on end request, but that's the host component's job to worry about.
            builder.Register<ContainerProvider>().As<IContainerProvider>().ContainerScoped();

            builder.Register<ExtensionManager>().As<IExtensionManager>().SingletonScoped();
            builder.Register<AreaExtensionLoader>().As<IExtensionLoader>().SingletonScoped();
            builder.Register<CoreExtensionLoader>().As<IExtensionLoader>().SingletonScoped();
            builder.Register<ReferencedExtensionLoader>().As<IExtensionLoader>().SingletonScoped();
            builder.Register<PrecompiledExtensionLoader>().As<IExtensionLoader>().SingletonScoped();
            builder.Register<DynamicExtensionLoader>().As<IExtensionLoader>().SingletonScoped();

            builder.Register<PackageFolders>().As<IExtensionFolders>()
                .WithArguments(new NamedParameter("paths", new[] { "~/Core", "~/Packages" }))
                .SingletonScoped();
            builder.Register<AreaFolders>().As<IExtensionFolders>()
                .WithArguments(new NamedParameter("paths", new[] { "~/Areas" }))
                .SingletonScoped();
            builder.Register<ThemeFolders>().As<IExtensionFolders>()
                .WithArguments(new NamedParameter("paths", new[] { "~/Core", "~/Themes" }))
                .SingletonScoped();

            registrations(builder);

            return builder.Build();
        }

        public static IOrchardHost CreateHost(Action<ContainerBuilder> registrations) {
            var container = CreateHostContainer(registrations);
            var orchardHost = container.Resolve<IOrchardHost>();
            return orchardHost;
        }
    }
}
