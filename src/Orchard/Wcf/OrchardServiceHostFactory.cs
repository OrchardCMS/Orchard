using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using Autofac;
using Autofac.Core;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders;

namespace Orchard.Wcf {
    public class OrchardServiceHostFactory : ServiceHostFactory, IShim {
        public OrchardServiceHostFactory() {
            OrchardHostContainerRegistry.RegisterShim(this);
        }

        public IOrchardHostContainer HostContainer { get; set; }

        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses) {
            IComponentRegistration registration;
            if (constructorString == null) {
                throw new ArgumentNullException("constructorString");
            }

            if (constructorString == string.Empty) {
                throw new ArgumentOutOfRangeException("constructorString");
            }

            if (HostContainer == null) {
                throw new InvalidOperationException();
            }

            // Create work context
            IRunningShellTable runningShellTable = HostContainer.Resolve<IRunningShellTable>();
            ShellSettings shellSettings = runningShellTable.Match(baseAddresses.First().Host, baseAddresses.First().LocalPath);

            IOrchardHost orchardHost = HostContainer.Resolve<IOrchardHost>();
            ShellContext shellContext = orchardHost.GetShellContext(shellSettings);
            IWorkContextAccessor workContextAccessor = shellContext.LifetimeScope.Resolve<IWorkContextAccessor>();

            using (IWorkContextScope workContext = workContextAccessor.CreateWorkContextScope()) {

                ILifetimeScope lifetimeScope = workContext.Resolve<ILifetimeScope>();
                if (!lifetimeScope.ComponentRegistry.TryGetRegistration(new KeyedService(constructorString, typeof (object)), out registration)) {
                    Type serviceType = Type.GetType(constructorString, false);
                    if (serviceType != null) {
                        lifetimeScope.ComponentRegistry.TryGetRegistration(new TypedService(serviceType), out registration);
                    }
                }
            }

            if (registration == null) {
                throw new InvalidOperationException();
            }

            if (!registration.Activator.LimitType.IsClass) {
                throw new InvalidOperationException();
            }

            return CreateServiceHost(workContextAccessor, registration, registration.Activator.LimitType, baseAddresses);
        }

        private ServiceHost CreateServiceHost(IWorkContextAccessor workContextAccessor, IComponentRegistration registration, Type implementationType, Uri[] baseAddresses) {
            ServiceHost host = CreateServiceHost(implementationType, baseAddresses);

            host.Opening += delegate {
                host.Description.Behaviors.Add(new OrchardDependencyInjectionServiceBehavior(workContextAccessor, implementationType, registration));
            };

            return host;
        }
    }
}
