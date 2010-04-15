using System;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Indexed;
using Orchard.Environment.AutofacUtil.DynamicProxy2;
using Orchard.Environment.Topology.Models;

namespace Orchard.Environment.ShellBuilders {
    public class DefaultShellContainerFactory : IShellContainerFactory {
        private readonly ILifetimeScope _lifetimeScope;

        public DefaultShellContainerFactory(ILifetimeScope lifetimeScope) {
            _lifetimeScope = lifetimeScope;
        }

        public ILifetimeScope CreateContainer(ShellTopology topology) {
            var intermediateScope = _lifetimeScope.BeginLifetimeScope(
                builder => {
                    foreach (var item in topology.Modules) {
                        RegisterType(builder, item)
                            .Keyed<IModule>(item.Type)
                            .InstancePerDependency();
                    }
                });

            return intermediateScope.BeginLifetimeScope(
                "shell",
                builder => {
                    var dynamicProxyContext = new DynamicProxyContext();

                    builder.Register(ctx => dynamicProxyContext);
                    builder.Register(ctx => topology.ShellSettings);

                    var moduleIndex = intermediateScope.Resolve<IIndex<Type, IModule>>();
                    foreach (var item in topology.Modules) {
                        builder.RegisterModule(moduleIndex[item.Type]);
                    }

                    foreach (var item in topology.Dependencies) {
                        var registration = RegisterType(builder, item)
                            .EnableDynamicProxy(dynamicProxyContext)
                            .InstancePerDependency();

                        foreach (var interfaceType in item.Type.GetInterfaces().Where(itf => typeof(IDependency).IsAssignableFrom(itf))) {
                            registration = registration.As(interfaceType);
                            if (typeof(ISingletonDependency).IsAssignableFrom(interfaceType)) {
                                registration = registration.InstancePerMatchingLifetimeScope("shell");
                            }
                            else if (typeof(ITransientDependency).IsAssignableFrom(interfaceType)) {
                                registration = registration.InstancePerDependency();
                            }
                        }

                        foreach (var parameter in item.Parameters) {
                            registration = registration
                                .WithParameter(parameter.Name, parameter.Value)
                                .WithProperty(parameter.Name, parameter.Value);
                        }
                    }

                    foreach (var item in topology.Controllers) {
                        RegisterType(builder, item)
                            .EnableDynamicProxy(dynamicProxyContext)
                            .Keyed<IController>(item.AreaName + "|" + item.ControllerName)
                            .InstancePerDependency();
                    }
                });
        }

        private IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType(ContainerBuilder builder, ShellTopologyItem item) {
            return builder.RegisterType(item.Type)
                .WithProperty("ExtensionEntry", item.ExtensionEntry)
                .WithProperty("FeatureDescriptor", item.FeatureDescriptor)
                .WithProperty("ExtensionDescriptor", item.ExtensionDescriptor);
        }
    }
}
