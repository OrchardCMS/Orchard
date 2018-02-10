using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Web;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.FileSystems.AppData;
using Orchard.FileSystems.VirtualPath;
using Orchard.Mvc.ModelBinders;
using Orchard.Mvc.Routes;
using Orchard.Owin;
using Orchard.Tests.Environment.TestDependencies;
using Orchard.Tests.Stubs;
using Orchard.Tests.Utility;
using Orchard.WebApi.Routes;
using IModelBinderProvider = Orchard.Mvc.ModelBinders.IModelBinderProvider;

namespace Orchard.Tests.Environment {
    [TestFixture]
    public class DefaultOrchardHostTests {
        private IContainer _container;
        private ILifetimeScope _lifetime;
        private RouteCollection _routeCollection;
        private ModelBinderDictionary _modelBinderDictionary;
        private ControllerBuilder _controllerBuilder;
        private ViewEngineCollection _viewEngineCollection;

        [SetUp]
        public void Init() {
            var clock = new StubClock();
            var appDataFolder = new StubAppDataFolder(clock);

            _controllerBuilder = new ControllerBuilder();
            _routeCollection = new RouteCollection();
            _modelBinderDictionary = new ModelBinderDictionary();
            _viewEngineCollection = new ViewEngineCollection { new WebFormViewEngine() };

            _container = OrchardStarter.CreateHostContainer(
                builder => {
                    builder.RegisterInstance(new StubShellSettingsLoader()).As<IShellSettingsManager>();
                    builder.RegisterType<RoutePublisher>().As<IRoutePublisher>();
                    builder.RegisterType<ModelBinderPublisher>().As<IModelBinderPublisher>();
                    builder.RegisterType<ShellContextFactory>().As<IShellContextFactory>();
                    builder.RegisterType<StubExtensionManager>().As<IExtensionManager>();
                    builder.RegisterType<StubVirtualPathMonitor>().As<IVirtualPathMonitor>();
                    builder.RegisterInstance(appDataFolder).As<IAppDataFolder>();
                    builder.RegisterInstance(_controllerBuilder);
                    builder.RegisterInstance(_routeCollection);
                    builder.RegisterInstance(_modelBinderDictionary);
                    builder.RegisterInstance(_viewEngineCollection);
                    builder.RegisterAutoMocking()
                        .Ignore<IExtensionFolders>()
                        .Ignore<IRouteProvider>()
                        .Ignore<IHttpRouteProvider>()
                        .Ignore<IModelBinderProvider>()
                        .Ignore<IWorkContextEvents>()
                        .Ignore<IOwinMiddlewareProvider>();
                });
            _lifetime = _container.BeginLifetimeScope();

            _container.Mock<IContainerProvider>()
                .SetupGet(cp => cp.ApplicationContainer).Returns(_container);
            _container.Mock<IContainerProvider>()
                .SetupGet(cp => cp.RequestLifetime).Returns(_lifetime);
            _container.Mock<IContainerProvider>()
                .Setup(cp => cp.EndRequestLifetime()).Callback(() => _lifetime.Dispose());

            _container.Mock<IShellDescriptorManager>()
                .Setup(cp => cp.GetShellDescriptor()).Returns(default(ShellDescriptor));

            _container.Mock<IOrchardShellEvents>()
                .Setup(e => e.Activated());

            _container.Mock<IOrchardShellEvents>()
                .Setup(e => e.Terminating()).Callback(() => new object());
        }

        public class StubExtensionManager : IExtensionManager {
            public ExtensionDescriptor GetExtension(string name) {
                throw new NotImplementedException();
            }

            public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
                var ext = new ExtensionDescriptor { Id = "Orchard.Framework" };
                ext.Features = new[] { new FeatureDescriptor { Extension = ext, Id = ext.Id } };
                yield return ext;

                var settings = new ExtensionDescriptor { Id = "Settings" };
                settings.Features = new[] { new FeatureDescriptor { Extension = settings, Id = settings.Id } };
                yield return settings;
            }

            public IEnumerable<FeatureDescriptor> AvailableFeatures() {
                // note - doesn't order properly
                return AvailableExtensions().SelectMany(ed => ed.Features);
            }

            public IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors) {
                foreach (var descriptor in featureDescriptors) {
                    if (descriptor.Id == "Orchard.Framework") {
                        yield return FrameworkFeature(descriptor);
                    }
                }
            }

            private Feature FrameworkFeature(FeatureDescriptor descriptor) {
                return new Feature {
                    Descriptor = descriptor,
                    ExportedTypes = new[] {
                        typeof (TestDependency),
                        typeof (TestSingletonDependency),
                        typeof (TestTransientDependency),
                    }
                };
            }

            public void Monitor(Action<IVolatileToken> monitor) {
                throw new NotImplementedException();
            }
        }

        public class StubShellSettingsLoader : IShellSettingsManager {
            private readonly List<ShellSettings> _shellSettings = new List<ShellSettings> { new ShellSettings { Name = ShellSettings.DefaultName, State = TenantState.Running } };

            public IEnumerable<ShellSettings> LoadSettings() {
                return _shellSettings.AsEnumerable();
            }

            public void SaveSettings(ShellSettings settings) {
                _shellSettings.Add(settings);
            }
        }

        [Test, Ignore("containers are disposed when calling BeginRequest, maybe by the StubVirtualPathMonitor")]
        public void NormalDependenciesShouldBeUniquePerRequestContainer() {
            var host = _lifetime.Resolve<IOrchardHost>();
            var container1 = host.CreateShellContainer_Obsolete();
            ((IShellDescriptorManagerEventHandler)host).Changed(null, ShellSettings.DefaultName);
            host.BeginRequest(); // force reloading the shell
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
            return ((DefaultOrchardHost)host)
                .Current
                .Single(x => x.Settings.Name == ShellSettings.DefaultName)
                .LifetimeScope;
        }

        public static IOrchardShell CreateShell_Obsolete(this IOrchardHost host) {
            return host.CreateShellContainer_Obsolete().Resolve<IOrchardShell>();
        }
    }
}