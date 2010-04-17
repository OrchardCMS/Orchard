using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Web;
using Moq;
using NUnit.Framework;
using Orchard.Environment;
using Orchard.Environment.AutofacUtil;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;
using Orchard.Mvc;
using Orchard.Mvc.ModelBinders;
using Orchard.Mvc.Routes;
using Orchard.Tests.Environment.TestDependencies;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Environment {
    [TestFixture]
    public class DefaultOrchardHostTests {
        private IContainer _container;
        private ILifetimeScope _lifetime;
        private RouteCollection _routeCollection;
        private ModelBinderDictionary _modelBinderDictionary;
        private ControllerBuilder _controllerBuilder;

        [SetUp]
        public void Init() {
            _controllerBuilder = new ControllerBuilder();
            _routeCollection = new RouteCollection();
            _modelBinderDictionary = new ModelBinderDictionary();

            _container = OrchardStarter.CreateHostContainer(
                builder => {
                    //builder.RegisterModule(new ImplicitCollectionSupportModule());
                    builder.RegisterType<StubContainerProvider>().As<IContainerProvider>().InstancePerLifetimeScope();
                    builder.RegisterType<StubCompositionStrategy>().As<ICompositionStrategy_Obsolete>().As<ICompositionStrategy>().InstancePerLifetimeScope();
                    builder.RegisterType<DefaultOrchardHost>().As<IOrchardHost>().SingleInstance();
                    builder.RegisterType<RoutePublisher>().As<IRoutePublisher>();
                    builder.RegisterType<ModelBinderPublisher>().As<IModelBinderPublisher>();
                    builder.RegisterType<DefaultShellContextFactory>().As<IShellContextFactory>();                    
                    builder.RegisterInstance(_controllerBuilder);
                    builder.RegisterInstance(_routeCollection);
                    builder.RegisterInstance(_modelBinderDictionary);
                    builder.RegisterInstance(new ViewEngineCollection { new WebFormViewEngine() });
                    builder.RegisterInstance(new StuExtensionManager()).As<IExtensionManager>();
                    builder.RegisterInstance(new Mock<IHackInstallationGenerator>().Object);
                    builder.RegisterInstance(new StubShellSettingsLoader()).As<ITenantManager>();
                    builder.RegisterInstance(new Mock<ITopologyDescriptorCache>().Object);
                });
            _lifetime = _container.BeginLifetimeScope();
            var updater = new ContainerUpdater();
            updater.RegisterInstance(_container).SingleInstance();
            updater.Update(_lifetime);
        }

        public class StubShellSettingsLoader : ITenantManager {
            private readonly List<ShellSettings> _shellSettings = new List<ShellSettings>
                                                                   {new ShellSettings {Name = "testing"}};

            public IEnumerable<ShellSettings> LoadSettings() {
                return _shellSettings.AsEnumerable();
            }

            public void SaveSettings(ShellSettings settings) {
                _shellSettings.Add(settings);
            }
        }

        public class StuExtensionManager : IExtensionManager {
            public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
                return Enumerable.Empty<ExtensionDescriptor>();
            }

            public IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> features) {
                throw new NotImplementedException();
            }

            public Feature LoadFeature(FeatureDescriptor featureDescriptor) {
                throw new NotImplementedException();
            }

            public IEnumerable<ExtensionEntry> ActiveExtensions_Obsolete() {
                return Enumerable.Empty<ExtensionEntry>();
            }

            public ShellTopology_Obsolete GetExtensionsTopology() {
                throw new NotImplementedException();
            }

            public Feature LoadFeature(string featureName) {
                throw new NotImplementedException();
            }

            public void InstallExtension(string extensionType, HttpPostedFileBase extensionBundle) {
                throw new NotImplementedException();
            }

            public void UninstallExtension(string extensionType, string extensionName) {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void HostShouldSetControllerFactory() {
            var host = _lifetime.Resolve<IOrchardHost>();

            Assert.That(_controllerBuilder.GetControllerFactory(), Is.TypeOf<DefaultControllerFactory>());
            host.Initialize();
            Assert.That(_controllerBuilder.GetControllerFactory(), Is.TypeOf<OrchardControllerFactory>());
        }

        public class StubCompositionStrategy : ICompositionStrategy_Obsolete, ICompositionStrategy {
            public IEnumerable<RecordDescriptor> GetRecordDescriptors() {
                return Enumerable.Empty<RecordDescriptor>();
            }

            public ShellTopology Compose(ShellTopologyDescriptor descriptor) {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void DifferentShellInstanceShouldBeReturnedAfterEachCreate() {
            var host = _lifetime.Resolve<IOrchardHost>();
            var runtime1 = host.CreateShell_Obsolete();
            var runtime2 = host.CreateShell_Obsolete();
            Assert.That(runtime1, Is.Not.SameAs(runtime2));
        }


        [Test]
        public void NormalDependenciesShouldBeUniquePerRequestContainer() {
            var host = _lifetime.Resolve<IOrchardHost>();
            var container1 = host.CreateShellContainer_Obsolete();
            var container2 = host.CreateShellContainer_Obsolete();
            var requestContainer1a = container1.BeginLifetimeScope();
            var requestContainer1b = container1.BeginLifetimeScope();
            var requestContainer2a = container2.BeginLifetimeScope();
            var requestContainer2b = container2.BeginLifetimeScope();

            var dep1 = container1.Resolve<ITestDependency>();
            var dep1a = requestContainer1a.Resolve<ITestDependency>();
            var dep1b = requestContainer1b.Resolve<ITestDependency>();
            var dep2 = container2.Resolve<ITestDependency>();
            var dep2a = requestContainer2a.Resolve<ITestDependency>();
            var dep2b = requestContainer2b.Resolve<ITestDependency>();

            Assert.That(dep1, Is.Not.SameAs(dep2));
            Assert.That(dep1, Is.Not.SameAs(dep1a));
            Assert.That(dep1, Is.Not.SameAs(dep1b));
            Assert.That(dep2, Is.Not.SameAs(dep2a));
            Assert.That(dep2, Is.Not.SameAs(dep2b));

            var again1 = container1.Resolve<ITestDependency>();
            var again1a = requestContainer1a.Resolve<ITestDependency>();
            var again1b = requestContainer1b.Resolve<ITestDependency>();
            var again2 = container2.Resolve<ITestDependency>();
            var again2a = requestContainer2a.Resolve<ITestDependency>();
            var again2b = requestContainer2b.Resolve<ITestDependency>();

            Assert.That(again1, Is.SameAs(dep1));
            Assert.That(again1a, Is.SameAs(dep1a));
            Assert.That(again1b, Is.SameAs(dep1b));
            Assert.That(again2, Is.SameAs(dep2));
            Assert.That(again2a, Is.SameAs(dep2a));
            Assert.That(again2b, Is.SameAs(dep2b));
        }
        [Test]
        public void SingletonDependenciesShouldBeUniquePerShell() {
            var host = _lifetime.Resolve<IOrchardHost>();
            var container1 = host.CreateShellContainer_Obsolete();
            var container2 = host.CreateShellContainer_Obsolete();
            var requestContainer1a = container1.BeginLifetimeScope();
            var requestContainer1b = container1.BeginLifetimeScope();
            var requestContainer2a = container2.BeginLifetimeScope();
            var requestContainer2b = container2.BeginLifetimeScope();

            var dep1 = container1.Resolve<ITestSingletonDependency>();
            var dep1a = requestContainer1a.Resolve<ITestSingletonDependency>();
            var dep1b = requestContainer1b.Resolve<ITestSingletonDependency>();
            var dep2 = container2.Resolve<ITestSingletonDependency>();
            var dep2a = requestContainer2a.Resolve<ITestSingletonDependency>();
            var dep2b = requestContainer2b.Resolve<ITestSingletonDependency>();

            //Assert.That(dep1, Is.Not.SameAs(dep2));
            Assert.That(dep1, Is.SameAs(dep1a));
            Assert.That(dep1, Is.SameAs(dep1b));
            Assert.That(dep2, Is.SameAs(dep2a));
            Assert.That(dep2, Is.SameAs(dep2b));
        }
        [Test]
        public void TransientDependenciesShouldBeUniquePerResolve() {
            var host = _lifetime.Resolve<IOrchardHost>();
            var container1 = host.CreateShellContainer_Obsolete();
            var container2 = host.CreateShellContainer_Obsolete();
            var requestContainer1a = container1.BeginLifetimeScope();
            var requestContainer1b = container1.BeginLifetimeScope();
            var requestContainer2a = container2.BeginLifetimeScope();
            var requestContainer2b = container2.BeginLifetimeScope();

            var dep1 = container1.Resolve<ITestTransientDependency>();
            var dep1a = requestContainer1a.Resolve<ITestTransientDependency>();
            var dep1b = requestContainer1b.Resolve<ITestTransientDependency>();
            var dep2 = container2.Resolve<ITestTransientDependency>();
            var dep2a = requestContainer2a.Resolve<ITestTransientDependency>();
            var dep2b = requestContainer2b.Resolve<ITestTransientDependency>();

            Assert.That(dep1, Is.Not.SameAs(dep2));
            Assert.That(dep1, Is.Not.SameAs(dep1a));
            Assert.That(dep1, Is.Not.SameAs(dep1b));
            Assert.That(dep2, Is.Not.SameAs(dep2a));
            Assert.That(dep2, Is.Not.SameAs(dep2b));

            var again1 = container1.Resolve<ITestTransientDependency>();
            var again1a = requestContainer1a.Resolve<ITestTransientDependency>();
            var again1b = requestContainer1b.Resolve<ITestTransientDependency>();
            var again2 = container2.Resolve<ITestTransientDependency>();
            var again2a = requestContainer2a.Resolve<ITestTransientDependency>();
            var again2b = requestContainer2b.Resolve<ITestTransientDependency>();

            Assert.That(again1, Is.Not.SameAs(dep1));
            Assert.That(again1a, Is.Not.SameAs(dep1a));
            Assert.That(again1b, Is.Not.SameAs(dep1b));
            Assert.That(again2, Is.Not.SameAs(dep2));
            Assert.That(again2a, Is.Not.SameAs(dep2a));
            Assert.That(again2b, Is.Not.SameAs(dep2b));

        }
    }

    public static class TextExtensions {
        public static ILifetimeScope CreateShellContainer_Obsolete(this IOrchardHost host) {
            return ((DefaultOrchardHost) host)
                .Current
                .Single(x => x.Settings.Name == "Default")
                .LifetimeScope;
        }

        public static IOrchardShell CreateShell_Obsolete(this IOrchardHost host) {
            return host.CreateShellContainer_Obsolete().Resolve<IOrchardShell>();
        }
    }
}