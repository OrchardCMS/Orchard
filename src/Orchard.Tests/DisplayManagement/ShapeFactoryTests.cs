using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using NUnit.Framework;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.Tests.DisplayManagement {
    [TestFixture]
    public class DefaultShapeBuilderTests {
        private IContainer _container;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<DefaultShapeBuilder>().As<IShapeBuilder>();
            _container = builder.Build();
        }

        
        [Test]
        public void ShapeHasAttributesType() {
            var factory = _container.Resolve<IShapeBuilder>();
            dynamic foo = factory.Build("Foo", ArgsUtility.Empty());
            ShapeAttributes attributes = foo.Attributes;
            Assert.That(attributes.Type, Is.EqualTo("Foo"));
        }

        [Test]
        public void CreateShapeWithNamedArguments() {
            var factory = _container.Resolve<IShapeBuilder>();
            var foo = factory.Build("Foo", ArgsUtility.Named(new { one = 1, two = "dos" }));
        }
    }
}
