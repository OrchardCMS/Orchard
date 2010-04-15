using System;
using System.Configuration;
using System.IO;
using System.Web.Hosting;
using Autofac;
using Autofac.Configuration;
using Autofac.Integration.Web;
using Orchard.Environment.AutofacUtil;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders;
using Orchard.Extensions;
using Orchard.Extensions.Loaders;

namespace Orchard.Environment {
    public static class OrchardStarter {
        public static IContainer CreateHostContainer(Action<ContainerBuilder> registrations) {
            var builder = new ContainerBuilder();

            // a single default host implementation is needed for bootstrapping a web app domain
            builder.RegisterType<DefaultOrchardHost>().As<IOrchardHost>().SingleInstance();
            builder.RegisterType<DefaultCompositionStrategy>().As<ICompositionStrategy_Obsolete>().SingleInstance();
            builder.RegisterType<DefaultShellContainerFactory>().As<IShellContainerFactory>().SingleInstance();
            builder.RegisterType<AppDataFolder>().As<IAppDataFolder>().SingleInstance();
            builder.RegisterType<DefaultTenantManager>().As<ITenantManager>().SingleInstance();
            builder.RegisterType<SafeModeShellContainerFactory>().As<IShellContainerFactory_Obsolete>().SingleInstance();

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

            
            var autofacSection = ConfigurationManager.GetSection(ConfigurationSettingsReader.DefaultSectionName);
            if (autofacSection != null)
                builder.RegisterModule(new ConfigurationSettingsReader());
            
            var optionalHostConfig = HostingEnvironment.MapPath("~/Config/Host.config");
            if (File.Exists(optionalHostConfig))
                builder.RegisterModule(new ConfigurationSettingsReader(ConfigurationSettingsReader.DefaultSectionName, optionalHostConfig));

            var container = builder.Build();

            var updater = new ContainerUpdater();
            updater.RegisterInstance(container);
            updater.Update(container);

            return container;
        }

        public static IOrchardHost CreateHost(Action<ContainerBuilder> registrations) {
            var container = CreateHostContainer(registrations);
            return container.Resolve<IOrchardHost>();
        }
    }
}
