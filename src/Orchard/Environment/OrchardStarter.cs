using System;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Integration.Web;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders;
using Orchard.Extensions;
using Orchard.Extensions.Loaders;
using Orchard.Mvc;

namespace Orchard.Environment {
    public static class OrchardStarter {
        public static IContainer CreateHostContainer(Action<ContainerBuilder> registrations) {
            var builder = new ContainerBuilder();

            // a single default host implementation is needed for bootstrapping a web app domain
            builder.RegisterType<DefaultOrchardHost>().As<IOrchardHost>().SingleInstance();
            builder.RegisterType<DefaultCompositionStrategy>().As<ICompositionStrategy>().SingleInstance();
            builder.RegisterType<DefaultShellContainerFactory>().As<IShellContainerFactory>().SingleInstance();
            builder.RegisterType<AppDataFolder>().As<IAppDataFolder>().SingleInstance();
            builder.RegisterType<ShellSettingsLoader>().As<IShellSettingsLoader>().SingleInstance();
            builder.RegisterType<SafeModeShellContainerFactory>().As<IShellContainerFactory>().SingleInstance();

            // The container provider gives you access to the lowest container at the time, 
            // and dynamically creates a per-request container. The EndRequestLifetime method
            // still needs to be called on end request, but that's the host component's job to worry about
            builder.RegisterType<ContainerProvider>().As<IContainerProvider>().InstancePerLifetimeScope();

            builder.RegisterType<ExtensionManager>().As<IExtensionManager>().SingleInstance();
            builder.RegisterType<AreaExtensionLoader>().As<IExtensionLoader>().SingleInstance();
            builder.RegisterType<CoreExtensionLoader>().As<IExtensionLoader>().SingleInstance();
            builder.RegisterType<ReferencedExtensionLoader>().As<IExtensionLoader>().SingleInstance();
            builder.RegisterType<PrecompiledExtensionLoader>().As<IExtensionLoader>().SingleInstance();
            builder.RegisterType<DynamicExtensionLoader>().As<IExtensionLoader>().SingleInstance();

            builder.RegisterType<ModuleFolders>().As<IExtensionFolders>()
                .WithParameter(new NamedParameter("paths", new[] { "~/Core", "~/Modules" }))
                .SingleInstance();
            builder.RegisterType<AreaFolders>().As<IExtensionFolders>()
                .WithParameter(new NamedParameter("paths", new[] { "~/Areas" }))
                .SingleInstance();
            builder.RegisterType<ThemeFolders>().As<IExtensionFolders>()
                .WithParameter(new NamedParameter("paths", new[] { "~/Core", "~/Themes" }))
                .SingleInstance();

            registrations(builder);

            return builder.Build();
        }

        public static IOrchardHost CreateHost(Action<ContainerBuilder> registrations) {
            var container = CreateHostContainer(registrations);
            var updater = new ContainerUpdater();
            updater.RegisterInstance(container);
            updater.Update(container);
            var orchardHost = container.Resolve<IOrchardHost>();

            return orchardHost;
        }
    }
}
