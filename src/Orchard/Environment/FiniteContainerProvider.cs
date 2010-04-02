using Autofac;
using Autofac.Integration.Web;
using Orchard.Environment.ShellBuilders;

namespace Orchard.Environment {

    class FiniteContainerProvider : IContainerProvider {
        public FiniteContainerProvider(ILifetimeScope applicationContainer) {
            //ApplicationContainer = applicationContainer;
            
            RequestLifetime = applicationContainer.BeginLifetimeScope();
            var builder = new ContainerUpdater();
            // also inject this instance in case anyone asks for the container provider
            builder.RegisterInstance(this).As<IContainerProvider>();
            builder.Update(RequestLifetime);
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