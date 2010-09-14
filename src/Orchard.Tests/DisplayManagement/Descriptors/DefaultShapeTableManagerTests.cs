using System;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Tests.DisplayManagement.Descriptors {
    [TestFixture]
    public class DefaultShapeTableManagerTests : ContainerTestBase {
        protected override void Register(Autofac.ContainerBuilder builder) {
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();

            builder.RegisterType<TestShapeProvider>().As<IShapeTableProvider>()
                .WithMetadata("Feature", TestFeature());
        }

        static Feature TestFeature() {
            return new Feature {
                Descriptor = new FeatureDescriptor {
                    Name = "Testing",
                    Dependencies = Enumerable.Empty<string>(),
                    Extension = new ExtensionDescriptor {
                        Name = "Testing",
                        ExtensionType = "Module",
                    }
                }
            };
        }

        public class TestShapeProvider : IShapeTableProvider {
            public void Discover(ShapeTableBuilder builder) {
                builder.Describe("Hello");
            }
        }

        [Test]
        public void ManagerCanBeResolved() {
            var manager = _container.Resolve<IShapeTableManager>();
            Assert.That(manager, Is.Not.Null);
        }

        [Test]
        public void DefaultShapeTableIsReturnedForNullOrEmpty() {
            var manager = _container.Resolve<IShapeTableManager>();
            var shapeTable1 = manager.GetShapeTable(null);
            var shapeTable2 = manager.GetShapeTable(string.Empty);
            Assert.That(shapeTable1.Descriptors["Hello"], Is.Not.Null);
            Assert.That(shapeTable2.Descriptors["Hello"], Is.Not.Null);
        }
    }
}