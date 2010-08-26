using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using NUnit.Framework;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.Tests.DisplayManagement {
    [TestFixture]
    public class DefaultShapeBuilderTests {
        private IContainer _container;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            _container = builder.Build();
        }

        
        [Test]
        public void ShapeHasAttributesType() {
            var factory = _container.Resolve<IShapeFactory>();
            dynamic foo = factory.Create("Foo", ArgsUtility.Empty());
            ShapeAttributes attributes = foo.Attributes;
            Assert.That(attributes.Type, Is.EqualTo("Foo"));
        }

        [Test]
        public void CreateShapeWithNamedArguments() {
            var factory = _container.Resolve<IShapeFactory>();
            var foo = factory.Create("Foo", ArgsUtility.Named(new { one = 1, two = "dos" }));
        }
    }
}
