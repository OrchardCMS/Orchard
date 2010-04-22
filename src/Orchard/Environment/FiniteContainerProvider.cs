using Autofac;
using Autofac.Integration.Web;
using Orchard.Environment.AutofacUtil;

namespace Orchard.Environment {

    class FiniteContainerProvider : IContainerProvider {
        public FiniteContainerProvider(ILifetimeScope applicationContainer) {
            ApplicationContainer = new LifetimeScopeContainer(applicationContainer);
            RequestLifetime = ApplicationContainer.BeginLifetimeScope("httpRequest");
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