using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Autofac;
using Autofac.Core;

namespace Orchard.Wcf {
    public class OrchardDependencyInjectionServiceBehavior : IServiceBehavior {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly Type _implementationType;
        private readonly IComponentRegistration _componentRegistration;

        public OrchardDependencyInjectionServiceBehavior(IWorkContextAccessor workContextAccessor, Type implementationType, IComponentRegistration componentRegistration) {
            if (workContextAccessor == null) {
                throw new ArgumentNullException("workContextAccessor");
            }

            if (implementationType == null) {
                throw new ArgumentNullException("implementationType");
            }

            if (componentRegistration == null) {
                throw new ArgumentNullException("componentRegistration");
            }

            _workContextAccessor = workContextAccessor;
            _implementationType = implementationType;
            _componentRegistration = componentRegistration;
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) {
            if (serviceDescription == null) {
                throw new ArgumentNullException("serviceDescription");
            }

            if (serviceHostBase == null) {
                throw new ArgumentNullException("serviceHostBase");
            }

            IEnumerable<string> source = serviceDescription.Endpoints.Where<ServiceEndpoint>(delegate(ServiceEndpoint ep) {
                return ep.Contract.ContractType.IsAssignableFrom(this._implementationType);
            }).Select<ServiceEndpoint, string>(delegate(ServiceEndpoint ep) {
                return ep.Contract.Name;
            });

            OrchardInstanceProvider provider = new OrchardInstanceProvider(this._workContextAccessor, this._componentRegistration);
            foreach (ChannelDispatcherBase base2 in serviceHostBase.ChannelDispatchers) {
                ChannelDispatcher dispatcher = base2 as ChannelDispatcher;
                if (dispatcher != null) {
                    foreach (EndpointDispatcher dispatcher2 in dispatcher.Endpoints) {
                        if (source.Contains<string>(dispatcher2.ContractName)) {
                            dispatcher2.DispatchRuntime.InstanceProvider = provider;
                        }
                    }
                    continue;
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) {
        }
    }
}
