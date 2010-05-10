using System;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using Autofac;
using Autofac.Builder;
using Autofac.Configuration;
using Autofac.Core;
using Autofac.Features.Indexed;
using Autofac.Integration.Web.Mvc;
using Orchard.Environment.AutofacUtil.DynamicProxy2;
using Orchard.Environment.Configuration;
using Orchard.Environment.Topology.Models;
using Orchard.Events;

namespace Orchard.Environment.ShellBuilders {

    public interface IShellContainerFactory {
        ILifetimeScope CreateContainer(ShellSettings settings, ShellTopology topology);
    }

    public class ShellContainerFactory : IShellContainerFactory {
        private readonly ILifetimeScope _lifetimeScope;

        public ShellContainerFactory(ILifetimeScope lifetimeScope) {
            _lifetimeScope = lifetimeScope;
        }

        public ILifetimeScope CreateContainer(ShellSettings settings, ShellTopology topology) {
            var intermediateScope = _lifetimeScope.BeginLifetimeScope(
                builder => {
                    foreach (var item in topology.Dependencies.Where(t => typeof(IModule).IsAssignableFrom(t.Type))) {
                        var registration = RegisterType(builder, item)
                            .Keyed<IModule>(item.Type)
                            .InstancePerDependency();

                        foreach (var parameter in item.Parameters) {
                            registration = registration
                                .WithParameter(parameter.Name, parameter.Value)
                                .WithProperty(parameter.Name, parameter.Value);
                        }
                    }
                });

            return intermediateScope.BeginLifetimeScope(
                "shell",
                builder => {
                    var dynamicProxyContext = new DynamicProxyContext();

                    builder.Register(ctx => dynamicProxyContext);
                    builder.Register(ctx => settings);
                    builder.Register(ctx => topology);

                    var moduleIndex = intermediateScope.Resolve<IIndex<Type, IModule>>();
                    foreach (var item in topology.Dependencies.Where(t => typeof(IModule).IsAssignableFrom(t.Type))) {
                        builder.RegisterModule(moduleIndex[item.Type]);
                    }

                    foreach (var item in topology.Dependencies.Where(t => typeof(IDependency).IsAssignableFrom(t.Type))) {
                        var registration = RegisterType(builder, item)
                            .EnableDynamicProxy(dynamicProxyContext)
                            .InstancePerLifetimeScope();

                        foreach (var interfaceType in item.Type.GetInterfaces().Where(itf => typeof(IDependency).IsAssignableFrom(itf))) {
                            registration = registration.As(interfaceType);
                            if (typeof(ISingletonDependency).IsAssignableFrom(interfaceType)) {
                                registration = registration.InstancePerMatchingLifetimeScope("shell");
                            }
                            else if (typeof(ITransientDependency).IsAssignableFrom(interfaceType)) {
                                registration = registration.InstancePerDependency();
                            }
                        }

                        if (typeof(IEventHandler).IsAssignableFrom(item.Type)) {
                            registration = registration.As(typeof(IEventHandler));
                        }

                        foreach (var parameter in item.Parameters) {
                            registration = registration
                                .WithParameter(parameter.Name, parameter.Value)
                                .WithProperty(parameter.Name, parameter.Value);
                        }
                    }

                    foreach (var item in topology.Controllers) {
                        var serviceKey = (item.AreaName + "/" + item.ControllerName).ToLowerInvariant();
                        RegisterType(builder, item)
                            .EnableDynamicProxy(dynamicProxyContext)
                            .Keyed<IController>(serviceKey)
                            .InstancePerDependency()
                            .InjectActionInvoker();
                    }

                    var optionalShellConfig = HostingEnvironment.MapPath("~/Config/Sites.config");
                    if (File.Exists(optionalShellConfig))
                        builder.RegisterModule(new ConfigurationSettingsReader(ConfigurationSettingsReader.DefaultSectionName, optionalShellConfig));

                    var optionalShellByNameConfig = HostingEnvironment.MapPath("~/Config/Sites." + settings.Name + ".config");
                    if (File.Exists(optionalShellByNameConfig))
                        builder.RegisterModule(new ConfigurationSettingsReader(ConfigurationSettingsReader.DefaultSectionName, optionalShellByNameConfig));
                });
        }

        private IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType(ContainerBuilder builder, ShellTopologyItem item) {
            return builder.RegisterType(item.Type)
                .WithProperty("Feature", item.Feature);
        }
    }
}
