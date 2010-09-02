using Autofac;
using NUnit.Framework;
using Orchard.Tests.Utility;

namespace Orchard.Tests {
    public class ContainerTestBase {

        protected IContainer _container;

        [SetUp]
        public virtual void Init() {
            var builder = new ContainerBuilder();
            Register(builder);
            _container = builder.Build();
            Resolve(_container);
        }

        protected virtual void Register(ContainerBuilder builder) { }
        protected virtual void Resolve(IContainer container) { }
    }
}