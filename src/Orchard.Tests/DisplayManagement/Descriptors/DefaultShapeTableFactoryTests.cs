using System;
using Autofac;
using NUnit.Framework;
using Orchard.DisplayManagement.Descriptors;

namespace Orchard.Tests.DisplayManagement.Descriptors {
    [TestFixture]
    public class DefaultShapeTableFactoryTests : ContainerTestBase {
        private IShapeTableFactory _factory;

        protected override void Register(ContainerBuilder builder) {
            builder.RegisterType<DefaultShapeTableFactory>().As<IShapeTableFactory>();
        }

        protected override void Resolve(IContainer container) {
            _factory = container.Resolve<IShapeTableFactory>();
        }

        [Test]
        public void FactoryIsResolved() {
            Assert.That(_factory, Is.Not.Null);
        }
    }
}
