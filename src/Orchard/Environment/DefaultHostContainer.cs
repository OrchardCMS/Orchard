using Autofac;

namespace Orchard.Environment {
    public class DefaultOrchardHostContainer : IOrchardHostContainer {
        private readonly IContainer _container;

        public DefaultOrchardHostContainer(IContainer container) {
            _container = container;
        }

        public T Resolve<T>() {
            return _container.Resolve<T>();
        }
    }
}