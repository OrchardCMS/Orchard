using System.Collections.Generic;
using System.Linq;
using Autofac;
using AutofacContrib.DynamicProxy2;

namespace Orchard.Environment {
    public class ExtensibleInterceptionModule : InterceptionModule {
        public ExtensibleInterceptionModule(IEnumerable<IComponentInterceptorProvider> providers)
            : base(new CombinedProvider(providers.Concat(new[] { new FlexibleInterceptorProvider() })), new FlexibleInterceptorAttacher()) {
        }

        class CombinedProvider : IComponentInterceptorProvider {
            private readonly IEnumerable<IComponentInterceptorProvider> _providers;

            public CombinedProvider(IEnumerable<IComponentInterceptorProvider> providers) {
                _providers = providers;
            }

            public IEnumerable<Service> GetInterceptorServices(IComponentDescriptor descriptor) {
                return _providers
                    .SelectMany(x => x.GetInterceptorServices(descriptor))
                    .Distinct()
                    .ToList();
            }
        }
    }
}