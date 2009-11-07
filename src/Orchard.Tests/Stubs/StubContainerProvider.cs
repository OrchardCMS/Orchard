using Autofac;
using Autofac.Integration.Web;

namespace Orchard.Tests.Stubs {
    public class StubContainerProvider : IContainerProvider {
        public StubContainerProvider(IContainer applicationContainer, IContainer requestContainer) {
            ApplicationContainer = applicationContainer;
            RequestContainer = requestContainer;
        }

        public void DisposeRequestContainer() {
            RequestContainer.Dispose();
        }

        public IContainer ApplicationContainer { get; set; }

        public IContainer RequestContainer { get; set; }
    }
}