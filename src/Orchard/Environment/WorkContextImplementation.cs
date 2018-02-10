using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Autofac;

namespace Orchard.Environment {
    class WorkContextImplementation : WorkContext {
        readonly IComponentContext _componentContext;
        readonly ConcurrentDictionary<string, Func<object>> _stateResolvers = new ConcurrentDictionary<string, Func<object>>();
        readonly IEnumerable<Lazy<IWorkContextStateProvider>> _workContextStateProviders;

        public WorkContextImplementation(IComponentContext componentContext) {
            _componentContext = componentContext;
            _workContextStateProviders = componentContext.Resolve<IEnumerable<Lazy<IWorkContextStateProvider>>>();
        }

        public override T Resolve<T>() {
            return _componentContext.Resolve<T>();
        }

        public override object Resolve(Type serviceType) {
            return _componentContext.Resolve(serviceType);
        }

        public override bool TryResolve<T>(out T service) {
            return _componentContext.TryResolve(out service);
        }

        public override bool TryResolve(Type serviceType, out object service) {
            return _componentContext.TryResolve(serviceType, out service);
        }

        public override T GetState<T>(string name) {
            var resolver = _stateResolvers.GetOrAdd(name, FindResolverForState<T>);
            return (T)resolver();
        }

        Func<object> FindResolverForState<T>(string name) {
            var resolver = _workContextStateProviders.Select(wcsp => wcsp.Value.Get<T>(name)).FirstOrDefault(value => !Equals(value, default(T)));

            if (resolver == null) {
                return () => default(T);
            }
            return () => resolver(this);
        }


        public override void SetState<T>(string name, T value) {
            _stateResolvers[name] = () => value;
        }
    }
}
