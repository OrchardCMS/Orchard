using Autofac;
using Autofac.Integration.Web;

namespace Orchard.Environment {

    class FiniteContainerProvider : IContainerProvider {
        public FiniteContainerProvider(IContainer applicationContainer) {
            // explicitly create a request container for the life of this object
            ApplicationContainer = applicationContainer;
            RequestContainer = applicationContainer.CreateInnerContainer();

            // also inject this instance in case anyone asks for the container provider
            RequestContainer.Build(builder => builder.Register(this).As<IContainerProvider>());
        }

        public void DisposeRequestContainer() {
            var disposeContainer = RequestContainer;
            RequestContainer = null;

            if (disposeContainer != null)
                disposeContainer.Dispose();
        }

        public IContainer ApplicationContainer { get; private set; }

        public IContainer RequestContainer { get; private set; }
    }
}