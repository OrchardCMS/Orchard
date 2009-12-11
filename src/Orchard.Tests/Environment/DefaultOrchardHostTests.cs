using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Web;
using Autofac.Modules;
using NUnit.Framework;
using Orchard.Environment;
using Orchard.Mvc;
using Orchard.Mvc.ModelBinders;
using Orchard.Mvc.Routes;
using Orchard.Extensions;
using Orchard.Tests.Environment.TestDependencies;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Environment {
    [TestFixture]
    public class DefaultOrchardHostTests {
        private IContainer _container;
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
                    builder.RegisterModule(new ImplicitCollectionSupportModule());
                    builder.Register<StubContainerProvider>().As<IContainerProvider>().ContainerScoped();
                    builder.Register<StubCompositionStrategy>().As<ICompositionStrategy>().ContainerScoped();
                    builder.Register<DefaultOrchardHost>().As<IOrchardHost>();
                    builder.Register<RoutePublisher>().As<IRoutePublisher>();
                    builder.Register<ModelBinderPublisher>().As<IModelBinderPublisher>();
                    builder.Register(_controllerBuilder);
                    builder.Register(_routeCollection);
                    builder.Register(_modelBinderDictionary);
                    builder.Register(new ViewEngineCollection { new WebFormViewEngine() });
                    builder.Register(new StuExtensionManager()).As<IExtensionManager>();
                });
        }

        public class StuExtensionManager : IExtensionManager {
            public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
                return Enumerable.Empty<ExtensionDescriptor>();
            }

            public IEnumerable<ExtensionEntry> ActiveExtensions() {
                return Enumerable.Empty<ExtensionEntry>();
            }

            public void InstallExtension(HttpPostedFileBase extensionBundle) {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void HostShouldSetControllerFactory() {
            var host = _container.Resolve<IOrchardHost>();

            Assert.That(_controllerBuilder.GetControllerFactory(), Is.TypeOf<DefaultControllerFactory>());
            host.Initialize();
            Assert.That(_controllerBuilder.GetControllerFactory(), Is.TypeOf<OrchardControllerFactory>());
        }

        public class StubCompositionStrategy : ICompositionStrategy {
            public IEnumerable<Assembly> GetAssemblies() {
                return Enumerable.Empty<Assembly>();
            }

            public IEnumerable<Type> GetModuleTypes() {
                return Enumerable.Empty<Type>();
            }

            public IEnumerable<Type> GetDependencyTypes() {
                return new[] {
                                 typeof (TestDependency),
                                 typeof (TestSingletonDependency),
                                 typeof(TestTransientDependency)
                             };
            }

            public IEnumerable<Type> GetRecordTypes() {
                return Enumerable.Empty<Type>();
            }
        }

        [Test]
        public void DifferentShellInstanceShouldBeReturnedAfterEachCreate() {
            var host = _container.Resolve<IOrchardHost>();
            var runtime1 = host.CreateShell();
            var runtime2 = host.CreateShell();
            Assert.That(runtime1, Is.Not.SameAs(runtime2));
        }


        [Test]
        public void NormalDependenciesShouldBeUniquePerRequestContainer() {
            var host = (DefaultOrchardHost)_container.Resolve<IOrchardHost>();
            var container1 = host.CreateShellContainer();
            var container2 = host.CreateShellContainer();
            var requestContainer1a = container1.CreateInnerContainer();
            var requestContainer1b = container1.CreateInnerContainer();
            var requestContainer2a = container2.CreateInnerContainer();
            var requestContainer2b = container2.CreateInnerContainer();

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
            var host = (DefaultOrchardHost)_container.Resolve<IOrchardHost>();
            var container1 = host.CreateShellContainer();
            var container2 = host.CreateShellContainer();
            var requestContainer1a = container1.CreateInnerContainer();
            var requestContainer1b = container1.CreateInnerContainer();
            var requestContainer2a = container2.CreateInnerContainer();
            var requestContainer2b = container2.CreateInnerContainer();

            var dep1 = container1.Resolve<ITestSingletonDependency>();
            var dep1a = requestContainer1a.Resolve<ITestSingletonDependency>();
            var dep1b = requestContainer1b.Resolve<ITestSingletonDependency>();
            var dep2 = container2.Resolve<ITestSingletonDependency>();
            var dep2a = requestContainer2a.Resolve<ITestSingletonDependency>();
            var dep2b = requestContainer2b.Resolve<ITestSingletonDependency>();

            Assert.That(dep1, Is.Not.SameAs(dep2));
            Assert.That(dep1, Is.SameAs(dep1a));
            Assert.That(dep1, Is.SameAs(dep1b));
            Assert.That(dep2, Is.SameAs(dep2a));
            Assert.That(dep2, Is.SameAs(dep2b));
        }
        [Test]
        public void TransientDependenciesShouldBeUniquePerResolve() {
            var host = (DefaultOrchardHost)_container.Resolve<IOrchardHost>();
            var container1 = host.CreateShellContainer();
            var container2 = host.CreateShellContainer();
            var requestContainer1a = container1.CreateInnerContainer();
            var requestContainer1b = container1.CreateInnerContainer();
            var requestContainer2a = container2.CreateInnerContainer();
            var requestContainer2b = container2.CreateInnerContainer();

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
}