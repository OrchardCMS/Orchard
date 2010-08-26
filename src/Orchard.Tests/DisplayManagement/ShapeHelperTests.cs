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
    public class ShapeHelperTests {
        private IContainer _container;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<ShapeHelperFactory>().As<IShapeHelperFactory>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            _container = builder.Build();

        }

        [Test]
        public void CreatingNewShapeTypeByName() {
            dynamic shape = _container.Resolve<IShapeHelperFactory>().CreateHelper();

            var alpha = shape.Alpha();

            Assert.That(alpha.Attributes.Type, Is.EqualTo("Alpha"));
        }

        [Test]
        public void CreatingShapeWithAdditionalNamedParameters() {
            dynamic shape = _container.Resolve<IShapeHelperFactory>().CreateHelper();

            var alpha = shape.Alpha(one: 1, two: "dos");

            Assert.That(alpha.Attributes.Type, Is.EqualTo("Alpha"));
            Assert.That(alpha.one, Is.EqualTo(1));
            Assert.That(alpha.two, Is.EqualTo("dos"));
        }

        [Test]
        public void WithPropertyBearingObjectInsteadOfNamedParameters() {
            dynamic shape = _container.Resolve<IShapeHelperFactory>().CreateHelper();

            var alpha = shape.Alpha(new { one = 1, two = "dos" });

            Assert.That(alpha.Attributes.Type, Is.EqualTo("Alpha"));
            Assert.That(alpha.one, Is.EqualTo(1));
            Assert.That(alpha.two, Is.EqualTo("dos"));
        }
    }
}
