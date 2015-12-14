using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Descriptors.ShapeAttributeStrategy;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment;
using Orchard.Environment.Extensions.Models;
using Orchard.Tests.Utility;

namespace Orchard.Tests.DisplayManagement.Descriptors {
    [TestFixture]
    public class ShapeAttributeBindingStrategyTests : ContainerTestBase {
        private Feature _testFeature;

        protected override void Register(ContainerBuilder builder) {
            if (builder == null) {
                throw new ArgumentNullException("builder");
            }
            builder.RegisterAutoMocking();
            _testFeature = new Feature {
                Descriptor = new FeatureDescriptor {
                    Id = "Testing",
                    Extension = new ExtensionDescriptor {
                        Id = "Testing",
                        ExtensionType = DefaultExtensionTypes.Module,
                    }
                }
            };
            builder.RegisterType<ShapeAttributeBindingStrategy>().As<IShapeTableProvider>();
            builder.RegisterInstance(new TestProvider()).WithMetadata("Feature", _testFeature);
            builder.RegisterInstance(new RouteCollection());
            builder.RegisterModule(new ShapeAttributeBindingModule());
        }

        protected override void Resolve(ILifetimeScope container) {
            // implementation resorts to orchard host to resolve "current scope" services
            container.Resolve<Mock<IOrchardHostContainer>>()
                .Setup(x => x.Resolve<IComponentContext>())
                .Returns(container);
        }

        class TestProvider {
            [Shape]
            public string Simple() {
                return "Simple";
            }

            [Shape("Renamed")]
            public string RenamedMethod() {
                return "Renamed";
            }
        }

        static IEnumerable<ShapeAlteration> GetAlterationBuilders(IShapeTableProvider strategy) {
            var builder = new ShapeTableBuilder(null);
            strategy.Discover(builder);
            return builder.BuildAlterations();
        }

        [Test]
        public void ShapeAttributeOccurrencesAreDetected() {
            var occurrences = _container.Resolve<IEnumerable<ShapeAttributeOccurrence>>();
            Assert.That(occurrences.Any(o => o.MethodInfo == typeof(TestProvider).GetMethod("Simple")));
        }

        [Test]
        public void InitializersHaveExpectedShapeTypeNames() {
            var strategy = _container.Resolve<IShapeTableProvider>();
            var initializers = GetAlterationBuilders(strategy);
            Assert.That(initializers.Any(i => i.ShapeType == "Simple"));
            Assert.That(initializers.Any(i => i.ShapeType == "Renamed"));
            Assert.That(initializers.Any(i => i.ShapeType == "RenamedMethod"), Is.False);
        }

        [Test]
        public void FeatureMetadataIsDetected() {
            var strategy = _container.Resolve<IShapeTableProvider>();
            var initializers = GetAlterationBuilders(strategy);
            Assert.That(initializers.All(i => i.Feature == _testFeature));
        }

        [Test]
        public void LifetimeScopeContainersHaveMetadata() {
            var strategy = _container.Resolve<IShapeTableProvider>();
            var initializers = GetAlterationBuilders(strategy);
            Assert.That(initializers.Any(i => i.ShapeType == "Simple"));

            var childContainer = _container.BeginLifetimeScope();

            var strategy2 = childContainer.Resolve<IShapeTableProvider>();
            var initializers2 = GetAlterationBuilders(strategy2);
            Assert.That(initializers2.Any(i => i.ShapeType == "Simple"));

            Assert.That(strategy, Is.Not.SameAs(strategy2));
        }

        [Test]
        public void BindingProvidedByStrategyInvokesMethod() {
            var initializers = GetAlterationBuilders(_container.Resolve<IShapeTableProvider>());

            var shapeDescriptor = initializers.Where(i => i.ShapeType == "Simple")
                .Aggregate(new ShapeDescriptor { ShapeType = "Simple" }, (d, i) => { i.Alter(d); return d; });

            var displayContext = new DisplayContext();
            var result = shapeDescriptor.Binding(displayContext);
            var result2 = shapeDescriptor.Binding.Invoke(displayContext);
            Assert.That(result.ToString(), Is.StringContaining("Simple"));
            Assert.That(result2.ToString(), Is.StringContaining("Simple"));
        }

    }
}
