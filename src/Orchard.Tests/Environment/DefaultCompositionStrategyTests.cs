using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Core;
using Moq;
using NUnit.Framework;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;
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

            var foo = topology.Dependencies.SingleOrDefault(t => t.Type == typeof(FooService1));
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


        [Test]
        public void DependenciesAreGivenParameters() {
            var descriptor = Build.TopologyDescriptor()
                .WithFeatures("Foo")
                .WithParameter<FooService1>("one", "two")
                .WithParameter<FooService1>("three", "four");

            _extensionDescriptors = new[] {
                Build.ExtensionDescriptor("Foo").WithFeatures("Foo"),
            };

            _featureTypes["Foo"] = new[] { typeof(FooService1) };

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var topology = compositionStrategy.Compose(descriptor);

            var foo = topology.Dependencies.SingleOrDefault(t => t.Type == typeof(FooService1));
            Assert.That(foo, Is.Not.Null);
            Assert.That(foo.Parameters.Count(), Is.EqualTo(2));
            Assert.That(foo.Parameters.Single(x => x.Name == "one").Value, Is.EqualTo("two"));
            Assert.That(foo.Parameters.Single(x => x.Name == "three").Value, Is.EqualTo("four"));
        }

        [Test]
        public void ModulesArePutIntoTopology() {
            var descriptor = Build.TopologyDescriptor().WithFeatures("Foo", "Bar");

            _extensionDescriptors = new[] {
                Build.ExtensionDescriptor("Foo").WithFeatures("Foo"),
                Build.ExtensionDescriptor("Bar").WithFeatures("Bar"),
            };

            _featureTypes["Foo"] = new[] { typeof(AlphaModule) };
            _featureTypes["Bar"] = new[] { typeof(BetaModule) };

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var topology = compositionStrategy.Compose(descriptor);

            var alpha = topology.Modules.Single(x => x.Type == typeof (AlphaModule));
            var beta = topology.Modules.Single(x => x.Type == typeof (BetaModule));

            Assert.That(alpha.Feature.FeatureDescriptor.Name, Is.EqualTo("Foo"));
            Assert.That(beta.Feature.FeatureDescriptor.Name, Is.EqualTo("Bar"));
        }

        public class AlphaModule : Module {
        }

        public class BetaModule : IModule {
            public void Configure(IComponentRegistry componentRegistry) {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void ControllersArePutIntoTopology() {
            var descriptor = Build.TopologyDescriptor().WithFeatures("Foo Plus", "Bar Minus");

            _extensionDescriptors = new[] {
                Build.ExtensionDescriptor("Foo").WithFeatures("Foo", "Foo Plus"),
                Build.ExtensionDescriptor("Bar").WithFeatures("Bar", "Bar Minus"),
            };

            _featureTypes["Foo"] = Enumerable.Empty<Type>();
            _featureTypes["Foo Plus"] = new[] { typeof(GammaController) };
            _featureTypes["Bar"] = Enumerable.Empty<Type>();
            _featureTypes["Bar Minus"] = new[] { typeof(DeltaController), typeof(EpsilonController) };

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var topology = compositionStrategy.Compose(descriptor);

            var gamma = topology.Controllers.Single(x => x.Type == typeof (GammaController));
            var delta = topology.Controllers.Single(x => x.Type == typeof (DeltaController));
            var epsilon = topology.Controllers.Single(x => x.Type == typeof (EpsilonController));

            Assert.That(gamma.Feature.FeatureDescriptor.Name, Is.EqualTo("Foo Plus"));
            Assert.That(gamma.AreaName, Is.EqualTo("Foo"));
            Assert.That(gamma.ControllerName, Is.EqualTo("Gamma"));

            Assert.That(delta.Feature.FeatureDescriptor.Name, Is.EqualTo("Bar Minus"));
            Assert.That(delta.AreaName, Is.EqualTo("Bar"));
            Assert.That(delta.ControllerName, Is.EqualTo("Delta"));

            Assert.That(epsilon.Feature.FeatureDescriptor.Name, Is.EqualTo("Bar Minus"));
            Assert.That(epsilon.AreaName, Is.EqualTo("Bar"));
            Assert.That(epsilon.ControllerName, Is.EqualTo("Epsilon"));
        }

        
        public class GammaController : Controller {
        }

        public class DeltaController : ControllerBase {
            protected override void ExecuteCore() {
                throw new NotImplementedException();
            }
        }

        public class EpsilonController : IController {
            public void Execute(RequestContext requestContext) {
                throw new NotImplementedException();
            }
        }
    }


    static class Build {

        public static ShellTopologyDescriptor TopologyDescriptor() {
            var descriptor = new ShellTopologyDescriptor {
                EnabledFeatures = Enumerable.Empty<TopologyFeature>(),
                Parameters = Enumerable.Empty<TopologyParameter>(),
            };
            return descriptor;
        }

        public static ShellTopologyDescriptor WithFeatures(this ShellTopologyDescriptor descriptor, params string[] names) {
            descriptor.EnabledFeatures = descriptor.EnabledFeatures.Concat(
                names.Select(name => new TopologyFeature { Name = name }));

            return descriptor;
        }

        public static ShellTopologyDescriptor WithParameter<TComponent>(this ShellTopologyDescriptor descriptor, string name, string value) {
            descriptor.Parameters = descriptor.Parameters.Concat(
                new[] { new TopologyParameter { Component = typeof(TComponent).FullName, Name = name, Value = value } });

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
