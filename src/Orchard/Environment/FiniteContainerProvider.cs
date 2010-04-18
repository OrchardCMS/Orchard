using Autofac;
using Autofac.Integration.Web;

namespace Orchard.Environment {

    class FiniteContainerProvider : IContainerProvider {
        public FiniteContainerProvider(ILifetimeScope applicationContainer) {
            //ApplicationContainer = applicationContainer;
            
            RequestLifetime = applicationContainer.BeginLifetimeScope(
                builder=> {
                    // also inject this instance in case anyone asks for the container provider
                    builder.Register(ctx => this).As<IContainerProvider>();
                });
        }

        public void EndRequestLifetime() {
            var disposeContainer = RequestLifetime;
            RequestLifetime = null;

            if (disposeContainer != null)
                disposeContainer.Dispose();
        }

        public IContainer ApplicationContainer { get; private set; }

        public ILifetimeScope RequestLifetime { get; private set; }
    }
}