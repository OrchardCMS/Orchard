using Autofac;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
using Orchard.Environment.Extensions;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.DisplayManagement {
    [TestFixture]
    public class ShapeFactoryTests {
        private IContainer _container;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<ShapeTableLocator>().As<IShapeTableLocator>();
            builder.RegisterType<StubExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();
            _container = builder.Build();
        }

        
        [Test]
        public void ShapeHasAttributesType() {
            var factory = _container.Resolve<IShapeFactory>();
            dynamic foo = factory.Create("Foo", ArgsUtility.Empty());
            ShapeMetadata metadata = foo.Metadata;
            Assert.That(metadata.Type, Is.EqualTo("Foo"));
        }

        [Test]
        public void CreateShapeWithNamedArguments() {
            var factory = _container.Resolve<IShapeFactory>();
            var foo = factory.Create("Foo", ArgsUtility.Named(new { one = 1, two = "dos" }));
        }
    }
}
