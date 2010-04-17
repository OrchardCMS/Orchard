using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Environment;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;
using Orchard.Extensions;
using Orchard.Extensions.Models;
using Orchard.Tests.Utility;

namespace Orchard.Tests.Environment {
    [TestFixture]
    public class DefaultCompositionStrategyTests {

        private IContainer _container;

        private IEnumerable<ExtensionDescriptor> _extensionDescriptors;
        private IDictionary<string, IEnumerable<Type>> _featureTypes;


        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<DefaultCompositionStrategy>().As<ICompositionStrategy>();
            builder.RegisterAutoMocking(MockBehavior.Strict);
            _container = builder.Build();

            _extensionDescriptors = Enumerable.Empty<ExtensionDescriptor>();
            _featureTypes = new Dictionary<string, IEnumerable<Type>>();

            _container.Mock<IExtensionManager>()
                .Setup(x => x.AvailableExtensions())
                .Returns(() => _extensionDescriptors);

            _container.Mock<IExtensionManager>()
                .Setup(x => x.LoadFeatures(It.IsAny<IEnumerable<FeatureDescriptor>>()))
                .Returns((IEnumerable<FeatureDescriptor> x) => StubLoadFeatures(x));
        }

        private IEnumerable<Feature> StubLoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors) {
            return featureDescriptors.Select(featureDescriptor => new Feature {
                FeatureDescriptor = featureDescriptor,
                ExportedTypes = _featureTypes[featureDescriptor.Name]
            });
        }

        [Test]
        public void TopologyIsNotNull() {
            var descriptor = Build.TopologyDescriptor();

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var topology = compositionStrategy.Compose(descriptor);

            Assert.That(topology, Is.Not.Null);
        }

        [Test]
        public void DependenciesFromFeatureArePutIntoTopology() {
            var descriptor = Build.TopologyDescriptor().WithFeatures("Foo", "Bar");

            _extensionDescriptors = new[] {
                Build.ExtensionDescriptor("Foo").WithFeatures("Foo"),
                Build.ExtensionDescriptor("Bar").WithFeatures("Bar"),
            };

            _featureTypes["Foo"] = new[] { typeof(FooService1) };
            _featureTypes["Bar"] = new[] { typeof(BarService1) };

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var topology = compositionStrategy.Compose(descriptor);

            Assert.That(topology, Is.Not.Null);
            Assert.That(topology.Dependencies.Count(), Is.EqualTo(2));

            var foo = topology.Dependencies.SingleOrDefault(t => t.Type == typeof (FooService1));            
            Assert.That(foo, Is.Not.Null);
            Assert.That(foo.Feature.FeatureDescriptor.Name, Is.EqualTo("Foo"));

            var bar = topology.Dependencies.SingleOrDefault(t => t.Type == typeof(BarService1));
            Assert.That(bar, Is.Not.Null);
            Assert.That(bar.Feature.FeatureDescriptor.Name, Is.EqualTo("Bar"));
        }

        public interface IFooService : IDependency {
        }

        public class FooService1 : IFooService {
        }

        public interface IBarService : IDependency {
        }

        public class BarService1 : IBarService {
        }
    }


    static class Build {

        public static ShellTopologyDescriptor TopologyDescriptor() {
            var descriptor = new ShellTopologyDescriptor {
                EnabledFeatures = Enumerable.Empty<TopologyFeature>(),
            };
            return descriptor;
        }

        public static ShellTopologyDescriptor WithFeatures(this ShellTopologyDescriptor descriptor, params string[] names) {
            descriptor.EnabledFeatures = descriptor.EnabledFeatures.Concat(
                names.Select(name => new TopologyFeature { Name = name }));

            return descriptor;
        }

        public static ExtensionDescriptor ExtensionDescriptor(string name) {
            var descriptor = new ExtensionDescriptor {
                Name = name,
                DisplayName = name,
                Features = Enumerable.Empty<FeatureDescriptor>(),
            };
            return descriptor;
        }

        public static ExtensionDescriptor WithFeatures(this ExtensionDescriptor descriptor, params string[] names) {
            descriptor.Features = descriptor.Features.Concat(
                names.Select(name => new FeatureDescriptor() { Name = name }));

            return descriptor;
        }
    }
}
