using System;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Tests.DisplayManagement.Descriptors {
    [TestFixture]
    public class DefaultShapeTableManagerTests : ContainerTestBase {
        protected override void Register(Autofac.ContainerBuilder builder) {
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();

            builder.RegisterType<TestShapeProvider>().As<IShapeTableProvider>()
                .WithMetadata("Feature", TestFeature())
                .As<TestShapeProvider>()
                .InstancePerLifetimeScope();
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

            public Action<ShapeTableBuilder> Discover = x => { };

            void IShapeTableProvider.Discover(ShapeTableBuilder builder) {
                builder.Describe("Hello");
                Discover(builder);
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

        [Test]
        public void CallbackAlterationsContributeToDescriptor() {
            Action<ShapeCreatingContext> cb1 = x => { };
            Action<ShapeCreatedContext> cb2 = x => { };
            Action<ShapeDisplayingContext> cb3 = x => { };
            Action<ShapeDisplayedContext> cb4 = x => { };

            _container.Resolve<TestShapeProvider>().Discover =
                builder => builder.Describe("Foo")
                               .OnCreating(cb1)
                               .OnCreated(cb2)
                               .OnDisplaying(cb3)
                               .OnDisplayed(cb4);

            var manager = _container.Resolve<IShapeTableManager>();

            var foo = manager.GetShapeTable(null).Descriptors["Foo"];

            Assert.That(foo.Creating.Single(), Is.SameAs(cb1));
            Assert.That(foo.Created.Single(), Is.SameAs(cb2));
            Assert.That(foo.Displaying.Single(), Is.SameAs(cb3));
            Assert.That(foo.Displayed.Single(), Is.SameAs(cb4));
        }
    }
}