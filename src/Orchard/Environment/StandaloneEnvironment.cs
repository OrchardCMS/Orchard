using System;
using Autofac;
using Autofac.Integration.Web;

namespace Orchard.Environment {
    public interface IStandaloneEnvironment : IDisposable {
        TService Resolve<TService>();
    }
    
    public class StandaloneEnvironment : IStandaloneEnvironment {
        private readonly IContainerProvider _containerProvider;

        public StandaloneEnvironment(IContainer applicationContainer) {
            _containerProvider = new FiniteContainerProvider(applicationContainer);
        }

        public TService Resolve<TService>() {
            return _containerProvider.RequestContainer.Resolve<TService>();
        }

        public void Dispose() {
            _containerProvider.DisposeRequestContainer();
        }
    }
}