using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Autofac;
using Autofac.Builder;
using Autofac.Configuration;
using Autofac.Core;
using Autofac.Features.Indexed;
using Orchard.Environment.AutofacUtil;
using Orchard.Environment.AutofacUtil.DynamicProxy2;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.Events;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.Environment.ShellBuilders {

    public interface IShellContainerFactory {
        ILifetimeScope CreateContainer(ShellSettings settings, ShellBlueprint blueprint);
    }

    public class ShellContainerFactory : IShellContainerFactory {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IShellContainerRegistrations _shellContainerRegistrations;

        public ShellContainerFactory(ILifetimeScope lifetimeScope, IShellContainerRegistrations shellContainerRegistrations) {
            _lifetimeScope = lifetimeScope;
            _shellContainerRegistrations = shellContainerRegistrations;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ILifetimeScope CreateContainer(ShellSettings settings, ShellBlueprint blueprint) {
            var intermediateScope = _lifetimeScope.BeginLifetimeScope(
                builder => {
                    foreach (var item in blueprint.Dependencies.Where(t => typeof(IModule).IsAssignableFrom(t.Type))) {
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
                    builder.Register(ctx => blueprint.Descriptor);
                    builder.Register(ctx => blueprint);

                    var concreteRegistrationNames = new ConcurrentDictionary<Type, ConcurrentBag<NamedRegistration>>();

                    var moduleIndex = intermediateScope.Resolve<IIndex<Type, IModule>>();
                    foreach (var item in blueprint.Dependencies.Where(t => typeof(IModule).IsAssignableFrom(t.Type))) {
                        builder.RegisterModule(moduleIndex[item.Type]);
                    }
                    
                    var itemsToBeRegistered = new ConcurrentQueue<ItemToBeRegistered>();
                    var decorators = new ConcurrentQueue<DecoratorRegistration>();

                    foreach (var item in blueprint.Dependencies.Where(t => typeof(IDependency).IsAssignableFrom(t.Type))) {
                        // Determine if this service is an IEventHandler
                        var isEventHandler = typeof (IEventHandler).IsAssignableFrom(item.Type);

                        // Harvest any interfaces that this service decorates
                        var decoratingTypes = item.Type.GetInterfaces()
                            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDecorator<>))
                            .Select(t => t.GetGenericArguments().First());

                        var isDecorator = decoratingTypes != null && decoratingTypes.Any();

                        if (isDecorator && isEventHandler) {
                            Logger.Error(string.Format("Type `{0}` is an IDecorator, but is also an IEventHandler. Decorating IEventHandlers is not currently supported. This decorator will not be registered.", item.Type.FullName));

                            continue;
                        }
                        
                        if (isDecorator) {
                            // If this service is a decorator, we need to determine which types it decorates
                            foreach (var itemToBeRegistered in itemsToBeRegistered) {
                                foreach (var interfaceType in decoratingTypes) {
                                    if (itemToBeRegistered.InterfaceTypes.Contains(interfaceType)) {
                                        if (itemToBeRegistered.DecoratedTypes == null) {
                                            itemToBeRegistered.DecoratedTypes = new List<Type>();
                                        }

                                        // Add to the collection of interfaces that are decorated only if this interface type has not previously been added
                                        if (!itemToBeRegistered.DecoratedTypes.Contains(interfaceType)) {
                                            itemToBeRegistered.DecoratedTypes.Add(interfaceType);
                                        }
                                    } 
                                }
                            }
                        }
                        
                        itemsToBeRegistered.Enqueue(new ItemToBeRegistered {Item = item, InterfaceTypes = GetInterfacesFromBlueprint(item), DecoratingTypes = decoratingTypes, IsEventHandler = isEventHandler});
                    }

                    foreach (var itemToBeRegistered in itemsToBeRegistered) {
                        var registration = RegisterType(builder, itemToBeRegistered.Item)
                            .AsSelf()
                            .EnableDynamicProxy(dynamicProxyContext)
                            .InstancePerLifetimeScope();

                        var registrationName = registration.ActivatorData.ImplementationType.FullName;

                        registration.Named(registrationName, itemToBeRegistered.Item.Type);

                        foreach (var interfaceType in itemToBeRegistered.InterfaceTypes) {

                            registration = SetRegistrationScope(interfaceType, registration);
                            
                            var itemIsDecorator = itemToBeRegistered.IsDecorator(interfaceType);
                            var itemIsDecorated = itemToBeRegistered.IsDecorated(interfaceType);

                            if (!itemIsDecorated && !itemIsDecorator) {
                                // This item is not decorated by another implementation of this interface type and is not a decorator.
                                // It should be registered as the implementation of this interface. The ensures that Autofac will resolve only a single implementation should there be one or more decorators.
                                registration = registration.As(interfaceType);
                            }

                            if (itemIsDecorator) {
                                // This item decorates the interface currently being registered.
                                // It needs to be added to the list of decorators so that is can be registered once all of the concrete implementations have been registered.
                                decorators.Enqueue(new DecoratorRegistration(interfaceType, itemToBeRegistered, itemIsDecorated));
                            }
                            else {
                                // This item is not a decorator.
                                // We need to add it to the list of concrete implementations. This allows us to know the names of the implementations that need to be decorated should a decorator for this interface exist.
                                AddConcreteRegistrationName(registrationName, interfaceType, itemToBeRegistered.Item.Type, concreteRegistrationNames);
                            }
                        }

                        if (itemToBeRegistered.IsEventHandler) {
                            var interfaces = itemToBeRegistered.Item.Type.GetInterfaces();
                            foreach (var interfaceType in interfaces) {

                                // register named instance for each interface, for efficient filtering inside event bus
                                // IEventHandler derived classes only
                                if (interfaceType.GetInterface(typeof(IEventHandler).Name) != null) {
                                    registration = registration.Named<IEventHandler>(interfaceType.Name);
                                }
                            }
                        }

                        foreach (var parameter in itemToBeRegistered.Item.Parameters) {
                            registration = registration
                                .WithParameter(parameter.Name, parameter.Value)
                                .WithProperty(parameter.Name, parameter.Value);
                        }
                    }

                    foreach (var decorator in decorators) {
                        // We need to ensure that there is an implementation of this service that can be decorated
                        if (!concreteRegistrationNames.ContainsKey(decorator.InterfaceType) || concreteRegistrationNames[decorator.InterfaceType] == null || !concreteRegistrationNames[decorator.InterfaceType].Any()) {
                            var exception = new OrchardFatalException(T("The only registered implementations of `{0}` are decorators. In order to avoid circular dependenices, there must be at least one implementation that is not marked with the `OrchardDecorator` attribute.", decorator.InterfaceType.FullName));
                            Logger.Fatal(exception, "Could not complete dependency registration as a circular dependency chain has been found.");

                            throw exception;
                        }

                        var decoratorNames = new ConcurrentBag<NamedRegistration>();

                        // For every implementation that can be decorated
                        foreach (var namedRegistration in concreteRegistrationNames[decorator.InterfaceType]) {
                            var registration = RegisterType(builder, decorator.ItemToBeRegistered.Item)
                                .AsSelf()
                                .EnableDynamicProxy(dynamicProxyContext)
                                .InstancePerLifetimeScope();

                            registration = SetRegistrationScope(decorator.InterfaceType, registration);

                            // Create a unique name for the decorator
                            var decoratorName = string.Format("{0}-{1}", namedRegistration.Name, decorator.ItemToBeRegistered.Item.Type.FullName);
                            registration = registration.Named(decoratorName, decorator.ItemToBeRegistered.Item.Type);

                            // Tell Autofac to resolve the decorated service with the implementation that has already been registered
                            registration = registration.WithParameter(
                                        (p, c) => p.ParameterType == decorator.InterfaceType,
                                        (p, c) => c.ResolveNamed(namedRegistration.Name, namedRegistration.ImplementationType));

                            if (!decorator.IsDecorated) {
                                // This is the last decorator in the stack, so register it as the implmentation of the interface that it is decorating
                                registration = registration.As(decorator.InterfaceType);
                            }

                            decoratorNames.Add(new NamedRegistration(decoratorName, decorator.ItemToBeRegistered.Item.Type));
                        }

                        // Update the collection of implmentation names that can be decorated to contain only the decorators (this allows us to stack decorators)
                        concreteRegistrationNames[decorator.InterfaceType] = decoratorNames;
                    }

                    foreach (var item in blueprint.Controllers) {
                        var serviceKeyName = (item.AreaName + "/" + item.ControllerName).ToLowerInvariant();
                        var serviceKeyType = item.Type;
                        RegisterType(builder, item)
                            .EnableDynamicProxy(dynamicProxyContext)
                            .Keyed<IController>(serviceKeyName)
                            .Keyed<IController>(serviceKeyType)
                            .WithMetadata("ControllerType", item.Type)
                            .InstancePerDependency();
                    }

                    foreach (var item in blueprint.HttpControllers) {
                        var serviceKeyName = (item.AreaName + "/" + item.ControllerName).ToLowerInvariant();
                        var serviceKeyType = item.Type;
                        RegisterType(builder, item)
                            .EnableDynamicProxy(dynamicProxyContext)
                            .Keyed<IHttpController>(serviceKeyName)
                            .Keyed<IHttpController>(serviceKeyType)
                            .WithMetadata("ControllerType", item.Type)
                            .InstancePerDependency();
                    }

                    // Register code-only registrations specific to a shell
                    _shellContainerRegistrations.Registrations(builder);

                    var optionalShellByNameConfig = HostingEnvironment.MapPath("~/Config/Sites." + settings.Name + ".config");
                    if (File.Exists(optionalShellByNameConfig)) {
                        builder.RegisterModule(new ConfigurationSettingsReader(ConfigurationSettingsReaderConstants.DefaultSectionName, optionalShellByNameConfig));
                    }
                    else {
                        var optionalShellConfig = HostingEnvironment.MapPath("~/Config/Sites.config");
                        if (File.Exists(optionalShellConfig))
                            builder.RegisterModule(new ConfigurationSettingsReader(ConfigurationSettingsReaderConstants.DefaultSectionName, optionalShellConfig));
                    }

                    var optionalComponentsConfig = HostingEnvironment.MapPath("~/Config/HostComponents.config");
                    if (File.Exists(optionalComponentsConfig))
                        builder.RegisterModule(new HostComponentsConfigModule(optionalComponentsConfig));
                });
        }

        private IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType(ContainerBuilder builder, ShellBlueprintItem item) {
            return builder.RegisterType(item.Type)
                .WithProperty("Feature", item.Feature)
                .WithMetadata("Feature", item.Feature);
        }

        private IEnumerable<Type> GetInterfacesFromBlueprint(DependencyBlueprint blueprint) {
            return blueprint.Type.GetInterfaces()
                .Where(itf => typeof(IDependency).IsAssignableFrom(itf)
                            && !typeof(IEventHandler).IsAssignableFrom(itf));
        }

        private IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> SetRegistrationScope(Type interfaceType, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> decoratorRegistration) {
            if (typeof (ISingletonDependency).IsAssignableFrom(interfaceType)) {
                decoratorRegistration = decoratorRegistration.InstancePerMatchingLifetimeScope("shell");
            }
            else if (typeof (IUnitOfWorkDependency).IsAssignableFrom(interfaceType)) {
                decoratorRegistration = decoratorRegistration.InstancePerMatchingLifetimeScope("work");
            }
            else if (typeof (ITransientDependency).IsAssignableFrom(interfaceType)) {
                decoratorRegistration = decoratorRegistration.InstancePerDependency();
            }
            return decoratorRegistration;
        }

        private void AddConcreteRegistrationName(string registrationName, Type interfaceType, Type implementationType, ConcurrentDictionary<Type, ConcurrentBag<NamedRegistration>> concreteRegistrationNames) {
            if (concreteRegistrationNames.ContainsKey(interfaceType)
                && concreteRegistrationNames[interfaceType] != null
                && !concreteRegistrationNames[interfaceType].Any(nr=>nr.Name==registrationName)) {
                concreteRegistrationNames[interfaceType].Add(new NamedRegistration(registrationName, implementationType));
            }
            else {
                concreteRegistrationNames[interfaceType] = new ConcurrentBag<NamedRegistration> {new NamedRegistration(registrationName, implementationType)};
            }
        }
        
        private class ItemToBeRegistered {
            public DependencyBlueprint Item { get; set; }
            public IEnumerable<Type> InterfaceTypes { get; set; }
            /// <summary>
            /// The types that this item decorates
            /// </summary>
            public IEnumerable<Type> DecoratingTypes { get; set; }
            /// <summary>
            /// The types that this item implements that are decorated by another item
            /// </summary>
            public IList<Type> DecoratedTypes { get; set; }
            public bool IsEventHandler { get; set; }

            public bool IsDecorated(Type interfaceType) {
                return DecoratedTypes != null && DecoratedTypes.Contains(interfaceType);
            }
            public bool IsDecorator(Type interfaceType) {
                return DecoratingTypes != null && DecoratingTypes.Contains(interfaceType);
            }
        }

        private class NamedRegistration {
            public NamedRegistration(string name, Type implementationType) {
                Name = name;
                ImplementationType = implementationType;
            }

            public string Name { get; private set; }
            public Type ImplementationType { get; private set; }
        }

        private class DecoratorRegistration {
            public DecoratorRegistration(Type interfaceType, ItemToBeRegistered itemToBeRegistered, bool isDecorated) {
                InterfaceType = interfaceType;
                ItemToBeRegistered = itemToBeRegistered;
                IsDecorated = isDecorated;
            }

            public Type InterfaceType { get; private set; }
            public ItemToBeRegistered ItemToBeRegistered { get; private set; }
            public bool IsDecorated { get; private set; }
        }
    }
}