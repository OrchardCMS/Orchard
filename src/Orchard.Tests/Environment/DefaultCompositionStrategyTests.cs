using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Core;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.ShellBuilders;
using Orchard.Tests.Environment.Utility;
using Orchard.Tests.Records;
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
            builder.RegisterType<CompositionStrategy>().As<ICompositionStrategy>();
            builder.RegisterAutoMocking(MockBehavior.Strict);
            _container = builder.Build();

            _extensionDescriptors = Enumerable.Empty<ExtensionDescriptor>();
            _featureTypes = new Dictionary<string, IEnumerable<Type>>();

            _container.Mock<IExtensionManager>()
                .Setup(x => x.AvailableExtensions())
                .Returns(() => _extensionDescriptors);

            _container.Mock<IExtensionManager>()
                .Setup(x => x.AvailableFeatures())
                .Returns(() => _extensionDescriptors.SelectMany(ext => ext.Features));

            _container.Mock<IExtensionManager>()
                .Setup(x => x.LoadFeatures(It.IsAny<IEnumerable<FeatureDescriptor>>()))
                .Returns((IEnumerable<FeatureDescriptor> x) => StubLoadFeatures(x));
        }

        private IEnumerable<Feature> StubLoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors) {
            return featureDescriptors.Select(featureDescriptor => new Feature {
                Descriptor = featureDescriptor,
                ExportedTypes = _featureTypes[featureDescriptor.Id]
            });
        }

        private static ShellSettings BuildDefaultSettings() {
            return new ShellSettings { Name = ShellSettings.DefaultName };
        }

        [Test]
        public void BlueprintIsNotNull() {
            var descriptor = Build.ShellDescriptor();

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var blueprint = compositionStrategy.Compose(BuildDefaultSettings(), descriptor);

            Assert.That(blueprint, Is.Not.Null);
        }


        [Test]
        public void DependenciesFromFeatureArePutIntoBlueprint() {
            var descriptor = Build.ShellDescriptor().WithFeatures("Foo", "Bar");

            _extensionDescriptors = new[] {
                Build.ExtensionDescriptor("Foo").WithFeatures("Foo"),
                Build.ExtensionDescriptor("Bar").WithFeatures("Bar"),
            };

            _featureTypes["Foo"] = new[] { typeof(FooService1) };
            _featureTypes["Bar"] = new[] { typeof(BarService1) };

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var blueprint = compositionStrategy.Compose(BuildDefaultSettings(), descriptor);

            Assert.That(blueprint, Is.Not.Null);
            Assert.That(blueprint.Dependencies.Count(), Is.EqualTo(2));

            var foo = blueprint.Dependencies.SingleOrDefault(t => t.Type == typeof(FooService1));
            Assert.That(foo, Is.Not.Null);
            Assert.That(foo.Feature.Descriptor.Id, Is.EqualTo("Foo"));

            var bar = blueprint.Dependencies.SingleOrDefault(t => t.Type == typeof(BarService1));
            Assert.That(bar, Is.Not.Null);
            Assert.That(bar.Feature.Descriptor.Id, Is.EqualTo("Bar"));
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
            var descriptor = Build.ShellDescriptor()
                .WithFeatures("Foo")
                .WithParameter<FooService1>("one", "two")
                .WithParameter<FooService1>("three", "four");

            _extensionDescriptors = new[] {
                Build.ExtensionDescriptor("Foo").WithFeatures("Foo"),
            };

            _featureTypes["Foo"] = new[] { typeof(FooService1) };

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var blueprint = compositionStrategy.Compose(BuildDefaultSettings(), descriptor);

            var foo = blueprint.Dependencies.SingleOrDefault(t => t.Type == typeof(FooService1));
            Assert.That(foo, Is.Not.Null);
            Assert.That(foo.Parameters.Count(), Is.EqualTo(2));
            Assert.That(foo.Parameters.Single(x => x.Name == "one").Value, Is.EqualTo("two"));
            Assert.That(foo.Parameters.Single(x => x.Name == "three").Value, Is.EqualTo("four"));
        }

        [Test]
        public void ModulesArePutIntoBlueprint() {
            var descriptor = Build.ShellDescriptor().WithFeatures("Foo", "Bar");

            _extensionDescriptors = new[] {
                Build.ExtensionDescriptor("Foo").WithFeatures("Foo"),
                Build.ExtensionDescriptor("Bar").WithFeatures("Bar"),
            };

            _featureTypes["Foo"] = new[] { typeof(AlphaModule) };
            _featureTypes["Bar"] = new[] { typeof(BetaModule) };

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var blueprint = compositionStrategy.Compose(BuildDefaultSettings(), descriptor);

            var alpha = blueprint.Dependencies.Single(x => x.Type == typeof(AlphaModule));
            var beta = blueprint.Dependencies.Single(x => x.Type == typeof(BetaModule));

            Assert.That(alpha.Feature.Descriptor.Id, Is.EqualTo("Foo"));
            Assert.That(beta.Feature.Descriptor.Id, Is.EqualTo("Bar"));
        }

        public class AlphaModule : Module {
        }

        public class BetaModule : IModule {
            public void Configure(IComponentRegistry componentRegistry) {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void ControllersArePutIntoBlueprintWithAreaAndControllerName() {
            var descriptor = Build.ShellDescriptor().WithFeatures("Foo Plus", "Bar Minus");

            _extensionDescriptors = new[] {
                Build.ExtensionDescriptor("MyCompany.Foo", "Foo").WithFeatures("Foo", "Foo Plus"),
                Build.ExtensionDescriptor("Bar").WithFeatures("Bar", "Bar Minus"),
            };

            _featureTypes["Foo"] = Enumerable.Empty<Type>();
            _featureTypes["Foo Plus"] = new[] { typeof(GammaController) };
            _featureTypes["Bar"] = Enumerable.Empty<Type>();
            _featureTypes["Bar Minus"] = new[] { typeof(DeltaController), typeof(EpsilonController) };

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var blueprint = compositionStrategy.Compose(BuildDefaultSettings(), descriptor);

            var gamma = blueprint.Controllers.Single(x => x.Type == typeof(GammaController));
            var delta = blueprint.Controllers.Single(x => x.Type == typeof(DeltaController));
            var epsilon = blueprint.Controllers.Single(x => x.Type == typeof(EpsilonController));

            Assert.That(gamma.Feature.Descriptor.Id, Is.EqualTo("Foo Plus"));
            Assert.That(gamma.AreaName, Is.EqualTo("MyCompany.Foo"));
            Assert.That(gamma.ControllerName, Is.EqualTo("Gamma"));

            Assert.That(delta.Feature.Descriptor.Id, Is.EqualTo("Bar Minus"));
            Assert.That(delta.AreaName, Is.EqualTo("Bar"));
            Assert.That(delta.ControllerName, Is.EqualTo("Delta"));

            Assert.That(epsilon.Feature.Descriptor.Id, Is.EqualTo("Bar Minus"));
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


        [Test]
        public void RecordsArePutIntoBlueprintWithTableName() {
            var descriptor = Build.ShellDescriptor().WithFeatures("Foo Plus", "Bar", "Bar Minus");

            _extensionDescriptors = new[] {
                Build.ExtensionDescriptor("MyCompany.Foo", "Foo").WithFeatures("Foo", "Foo Plus"),
                Build.ExtensionDescriptor("Bar").WithFeatures("Bar", "Bar Minus"),
            };

            _featureTypes["Foo"] = Enumerable.Empty<Type>();
            _featureTypes["Foo Plus"] = new[] { typeof(FooRecord) };
            _featureTypes["Bar"] = new[] { typeof(BarRecord) };
            _featureTypes["Bar Minus"] = Enumerable.Empty<Type>();

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var blueprint = compositionStrategy.Compose(BuildDefaultSettings(), descriptor);

            var foo = blueprint.Records.Single(x => x.Type == typeof(FooRecord));
            var bar = blueprint.Records.Single(x => x.Type == typeof(BarRecord));

            Assert.That(foo.Feature.Descriptor.Id, Is.EqualTo("Foo Plus"));
            Assert.That(foo.TableName, Is.EqualTo("MyCompany_Foo_FooRecord"));

            Assert.That(bar.Feature.Descriptor.Id, Is.EqualTo("Bar"));
            Assert.That(bar.TableName, Is.EqualTo("Bar_BarRecord"));
        }

        [Test]
        public void CoreRecordsAreAddedAutomatically() {
            var descriptor = Build.ShellDescriptor().WithFeatures("Orchard.Framework");

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var blueprint = compositionStrategy.Compose(BuildDefaultSettings(), descriptor);

            var ct = blueprint.Records.Single(x => x.Type == typeof(ContentTypeRecord));
            var ci = blueprint.Records.Single(x => x.Type == typeof(ContentItemRecord));
            var civ = blueprint.Records.Single(x => x.Type == typeof(ContentItemVersionRecord));

            Assert.That(ct.Feature.Descriptor.Id, Is.EqualTo("Orchard.Framework"));
            Assert.That(ct.TableName, Is.EqualTo("Orchard_Framework_ContentTypeRecord"));

            Assert.That(ci.Feature.Descriptor.Id, Is.EqualTo("Orchard.Framework"));
            Assert.That(ci.TableName, Is.EqualTo("Orchard_Framework_ContentItemRecord"));

            Assert.That(civ.Feature.Descriptor.Id, Is.EqualTo("Orchard.Framework"));
            Assert.That(civ.TableName, Is.EqualTo("Orchard_Framework_ContentItemVersionRecord"));
        }

        [Test]
        public void DataPrefixChangesTableName() {
            var settings = BuildDefaultSettings();
            settings.DataTablePrefix = "Yadda";
            var descriptor = Build.ShellDescriptor().WithFeatures("Foo Plus", "Bar", "Bar Minus");

            _extensionDescriptors = new[] {
                Build.ExtensionDescriptor("MyCompany.Foo", "Foo").WithFeatures("Foo", "Foo Plus"),
                Build.ExtensionDescriptor("Bar").WithFeatures("Bar", "Bar Minus"),
            };

            _featureTypes["Foo"] = Enumerable.Empty<Type>();
            _featureTypes["Foo Plus"] = new[] { typeof(FooRecord) };
            _featureTypes["Bar"] = new[] { typeof(BarRecord) };
            _featureTypes["Bar Minus"] = Enumerable.Empty<Type>();

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var blueprint = compositionStrategy.Compose(settings, descriptor);

            var foo = blueprint.Records.Single(x => x.Type == typeof(FooRecord));
            var bar = blueprint.Records.Single(x => x.Type == typeof(BarRecord));

            Assert.That(foo.Feature.Descriptor.Id, Is.EqualTo("Foo Plus"));
            Assert.That(foo.TableName, Is.EqualTo("Yadda_MyCompany_Foo_FooRecord"));

            Assert.That(bar.Feature.Descriptor.Id, Is.EqualTo("Bar"));
            Assert.That(bar.TableName, Is.EqualTo("Yadda_Bar_BarRecord"));
        }

        [Test]
        public void FeatureReplacement() {
            var descriptor = Build.ShellDescriptor().WithFeatures("Bar");

            _extensionDescriptors = new[] {
                Build.ExtensionDescriptor("Foo").WithFeatures("Bar"),
            };

            _featureTypes["Bar"] = new[] { typeof(ReplacedStubType), typeof(StubType), typeof(ReplacedStubNestedType), typeof(StubNestedType) };

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var blueprint = compositionStrategy.Compose(BuildDefaultSettings(), descriptor);

            Assert.That(blueprint.Dependencies.Count(), Is.EqualTo(2));
            Assert.That(blueprint.Dependencies.FirstOrDefault(dependency => dependency.Type.Equals(typeof(StubType))), Is.Not.Null);
            Assert.That(blueprint.Dependencies.FirstOrDefault(dependency => dependency.Type.Equals(typeof(StubNestedType))), Is.Not.Null);
        }

        [OrchardSuppressDependency("Orchard.Tests.Environment.DefaultCompositionStrategyTests+ReplacedStubNestedType")]
        internal class StubNestedType : IDependency {}

        internal class ReplacedStubNestedType : IDependency {}
    }

    [OrchardSuppressDependency("Orchard.Tests.Environment.ReplacedStubType")]
    internal class StubType : IDependency { }

    internal class ReplacedStubType : IDependency { }
}