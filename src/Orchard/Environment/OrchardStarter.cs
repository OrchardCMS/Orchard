using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.Hosting;
using Autofac;
using Autofac.Configuration;
using Autofac.Core;
using Autofac.Integration.Web;
using Orchard.Environment.AutofacUtil;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.Topology;
using Orchard.Events;
using Orchard.Logging;

namespace Orchard.Environment {
    public static class OrchardStarter {
        public static IContainer CreateHostContainer(Action<ContainerBuilder> registrations) {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new LoggingModule());

            // a single default host implementation is needed for bootstrapping a web app domain
            builder.RegisterType<DefaultOrchardEventBus>().As<IEventBus>().SingleInstance();
            builder.RegisterType<AppDataFolder>().As<IAppDataFolder>().SingleInstance();

            builder.RegisterType<DefaultOrchardHost>().As<IOrchardHost>().SingleInstance();
            {
                builder.RegisterType<ShellSettingsManager>().As<IShellSettingsManager>().SingleInstance();

                builder.RegisterType<ShellContextFactory>().As<IShellContextFactory>().SingleInstance();
                {
                    builder.RegisterType<ShellDescriptorCache>().As<IShellDescriptorCache>().SingleInstance();

                    builder.RegisterType<CompositionStrategy>()
                        .As<ICompositionStrategy>()
                        .SingleInstance();
                    {
                        builder.RegisterType<ExtensionManager>().As<IExtensionManager>().SingleInstance();
                        {
                            builder.RegisterType<ModuleFolders>().As<IExtensionFolders>()
                                .WithParameter(new NamedParameter("paths", new[] { "~/Core", "~/Modules" }))
                                .SingleInstance();
                            builder.RegisterType<AreaFolders>().As<IExtensionFolders>()
                                .WithParameter(new NamedParameter("paths", new[] { "~/Areas" }))
                                .SingleInstance();
                            builder.RegisterType<ThemeFolders>().As<IExtensionFolders>()
                                .WithParameter(new NamedParameter("paths", new[] { "~/Core", "~/Themes" }))
                                .SingleInstance();

                            builder.RegisterType<AreaExtensionLoader>().As<IExtensionLoader>().SingleInstance();
                            builder.RegisterType<CoreExtensionLoader>().As<IExtensionLoader>().SingleInstance();
                            builder.RegisterType<ReferencedExtensionLoader>().As<IExtensionLoader>().SingleInstance();
                            builder.RegisterType<PrecompiledExtensionLoader>().As<IExtensionLoader>().SingleInstance();
                            builder.RegisterType<DynamicExtensionLoader>().As<IExtensionLoader>().SingleInstance();
                        }
                    }

                    builder.RegisterType<ShellContainerFactory>().As<IShellContainerFactory>().SingleInstance();
                }
            }

            builder.RegisterType<DefaultOrchardShell>().As<IOrchardShell>().InstancePerMatchingLifetimeScope("shell");

            // The container provider gives you access to the lowest container at the time, 
            // and dynamically creates a per-request container. The EndRequestLifetime method
            // still needs to be called on end request, but that's the host component's job to worry about
            builder.RegisterType<ContainerProvider>().As<IContainerProvider>().InstancePerLifetimeScope();



            registrations(builder);


            var autofacSection = ConfigurationManager.GetSection(ConfigurationSettingsReader.DefaultSectionName);
            if (autofacSection != null)
                builder.RegisterModule(new ConfigurationSettingsReader());

            var optionalHostConfig = HostingEnvironment.MapPath("~/Config/Host.config");
            if (File.Exists(optionalHostConfig))
                builder.RegisterModule(new ConfigurationSettingsReader(ConfigurationSettingsReader.DefaultSectionName, optionalHostConfig));

            builder
                .Register(ctx => new LifetimeScopeContainer(ctx.Resolve<ILifetimeScope>()))
                .As<IContainer>()
                .InstancePerMatchingLifetimeScope("shell");

            return builder.Build();
        }

        public class LifetimeScopeContainer : IContainer {
            private readonly ILifetimeScope _lifetimeScope;

            public LifetimeScopeContainer(ILifetimeScope lifetimeScope) {
                _lifetimeScope = lifetimeScope;
            }

            public object Resolve(IComponentRegistration registration, IEnumerable<Parameter> parameters) {
                return _lifetimeScope.Resolve(registration, parameters);
            }

            public IComponentRegistry ComponentRegistry {
                get { return _lifetimeScope.ComponentRegistry; }
            }

            public void Dispose() {                
            }

            public ILifetimeScope BeginLifetimeScope() {
                return _lifetimeScope.BeginLifetimeScope();
            }

            public ILifetimeScope BeginLifetimeScope(object tag) {
                return _lifetimeScope.BeginLifetimeScope(tag);
            }

            public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction) {
                return _lifetimeScope.BeginLifetimeScope(configurationAction);
            }

            public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction) {
                return _lifetimeScope.BeginLifetimeScope(tag, configurationAction);
            }

            public IDisposer Disposer {
                get { return _lifetimeScope.Disposer; }
            }

            public object Tag {
                get { return _lifetimeScope.Tag; }
            }
        }

        public static IOrchardHost CreateHost(Action<ContainerBuilder> registrations) {
            var container = CreateHostContainer(registrations);
            return container.Resolve<IOrchardHost>();
        }
    }
}
