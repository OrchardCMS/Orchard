using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Features.Indexed;
using Autofac.Features.Metadata;
using Castle.Core.Interceptor;
using NUnit.Framework;
using Orchard.Environment.AutofacUtil.DynamicProxy2;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.Topology.Models;
using Orchard.Extensions;
using Orchard.Extensions.Models;

namespace Orchard.Tests.Environment.ShellBuilders {
    [TestFixture]
    public class DefaultShellContainerFactoryTests {
        private IContainer _container;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<DefaultShellContainerFactory>().As<IShellContainerFactory>();
            builder.RegisterType<ComponentForHostContainer>();
            _container = builder.Build();
        }

        ShellTopology CreateTopology(params ShellTopologyItem[] items) {
            return new ShellTopology {
                ShellSettings = new ShellSettings { Name = "Default" },
                Modules = items.OfType<ModuleTopology>(),
                Dependencies = items.OfType<DependencyTopology>(),
                Controllers = items.OfType<ControllerTopology>(),
                Records = items.OfType<RecordTopology>(),
            };
        }

        ModuleTopology WithModule<T>() {
            return new ModuleTopology { Type = typeof(T) };
        }

        ControllerTopology WithController<T>(string areaName, string controllerName) {
            return new ControllerTopology { Type = typeof(T), AreaName = areaName, ControllerName = controllerName };
        }

        DependencyTopology WithDependency<T>() {
            return new DependencyTopology { Type = typeof(T), Parameters = Enumerable.Empty<TopologyParameter>() };
        }

        [Test]
        public void ShouldReturnChildLifetimeScopeNamedShell() {
            var topology = CreateTopology();
            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(topology);

            Assert.That(shellContainer.Tag, Is.EqualTo("shell"));

            var scope = (LifetimeScope)shellContainer;
            Assert.That(scope.RootLifetimeScope, Is.SameAs(_container.Resolve<ILifetimeScope>()));
            Assert.That(scope.RootLifetimeScope, Is.Not.SameAs(shellContainer.Resolve<ILifetimeScope>()));
        }



        [Test]
        public void ControllersAreRegisteredAsKeyedServices() {
            var topology = CreateTopology(
                WithModule<TestModule>(),
                WithController<TestController>("foo", "bar"));

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(topology);
            var controllers = shellContainer.Resolve<IIndex<string, IController>>();
            var controller = controllers["foo|bar"];
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller, Is.InstanceOf<TestController>());
        }

        public class TestController : Controller {
        }


        [Test]
        public void ModulesAreResolvedAndRegistered() {
            var topology = CreateTopology(
                WithModule<TestModule>(),
                WithController<TestController>("foo", "bar"));

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(topology);

            var controllerMetas = shellContainer.Resolve<IIndex<string, Meta<IController>>>();
            var metadata = controllerMetas["foo|bar"].Metadata;

            Assert.That(metadata["Hello"], Is.EqualTo("World"));
        }


        public class TestModule : Module {
            protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration) {
                registration.Metadata["Hello"] = "World";
            }
        }

        [Test]
        public void ModulesMayResolveHostServices() {
            var topology = CreateTopology(
                WithModule<ModuleUsingThatComponent>());

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(topology);
            Assert.That(shellContainer.Resolve<string>(), Is.EqualTo("Module was loaded"));
        }

        public class ComponentForHostContainer {

        }

        public class ModuleUsingThatComponent : Module {
            private readonly ComponentForHostContainer _di;

            public ModuleUsingThatComponent(ComponentForHostContainer di) {
                _di = di;
            }

            protected override void Load(ContainerBuilder builder) {
                builder.RegisterInstance("Module was loaded");
            }
        }

        [Test]
        public void DependenciesAreResolvable() {
            var topology = CreateTopology(
                WithDependency<TestDependency>());

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(topology);

            var testDependency = shellContainer.Resolve<ITestDependency>();
            Assert.That(testDependency, Is.Not.Null);
            Assert.That(testDependency, Is.InstanceOf<TestDependency>());
        }

        public interface ITestDependency : IDependency {

        }
        public class TestDependency : ITestDependency {
        }

        [Test]
        public void ExtraInformationCanDropIntoProperties() {
            var topology = CreateTopology(
                          WithDependency<TestDependency2>());

            topology.Dependencies.Single().FeatureDescriptor =
                new FeatureDescriptor { Name = "Hello" };

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(topology);

            var testDependency = shellContainer.Resolve<ITestDependency>();
            Assert.That(testDependency, Is.Not.Null);
            Assert.That(testDependency, Is.InstanceOf<TestDependency2>());

            var testDependency2 = (TestDependency2)testDependency;
            Assert.That(testDependency2.FeatureDescriptor, Is.Not.Null);
            Assert.That(testDependency2.FeatureDescriptor.Name, Is.EqualTo("Hello"));
        }

        public class TestDependency2 : ITestDependency {
            public FeatureDescriptor FeatureDescriptor { get; set; }
        }

        [Test]
        public void ParametersMayOrMayNotBeUsedAsPropertiesAndConstructorParameters() {
            var topology = CreateTopology(
                WithDependency<TestDependency3>());

            topology.Dependencies.Single().Parameters =
                new[] {
                          new TopologyParameter {Name = "alpha", Value = "-a-"},
                          new TopologyParameter {Name = "Beta", Value = "-b-"},
                          new TopologyParameter {Name = "Gamma", Value = "-g-"},
                      };

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(topology);

            var testDependency = shellContainer.Resolve<ITestDependency>();
            Assert.That(testDependency, Is.Not.Null);
            Assert.That(testDependency, Is.InstanceOf<TestDependency3>());

            var testDependency3 = (TestDependency3)testDependency;
            Assert.That(testDependency3.GetAlpha(), Is.EqualTo("-a-"));
            Assert.That(testDependency3.Beta, Is.EqualTo("-b-"));
            Assert.That(testDependency3.Delta, Is.EqualTo("y"));
        }

        public class TestDependency3 : ITestDependency {
            private readonly string _alpha;

            public TestDependency3(string alpha) {
                _alpha = alpha;
                Beta = "x";
                Delta = "y";
            }

            public string Beta { get; set; }
            public string Delta { get; set; }

            public string GetAlpha() {
                return _alpha;
            }
        }


        [Test]
        public void DynamicProxyIsInEffect() {
            var topology = CreateTopology(
                WithModule<ProxModule>(),
                WithDependency<ProxDependency>());

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(topology);

            var testDependency = shellContainer.Resolve<IProxDependency>();
            Assert.That(testDependency.Hello(), Is.EqualTo("Foo"));

            var topology2 = CreateTopology(
                WithDependency<ProxDependency>());

            var shellContainer2 = factory.CreateContainer(topology2);

            var testDependency2 = shellContainer2.Resolve<IProxDependency>();
            Assert.That(testDependency2.Hello(), Is.EqualTo("World"));
        }

        public interface IProxDependency : IDependency {
            string Hello();
        }

        public class ProxDependency : IProxDependency {
            public virtual string Hello() {
                return "World";
            }
        }

        public class ProxIntercept : IInterceptor {
            public void Intercept(IInvocation invocation) {
                invocation.ReturnValue = "Foo";
            }
        }

        public class ProxModule : Module {
            protected override void Load(ContainerBuilder builder) {
                builder.RegisterType<ProxIntercept>();
            }

            protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration) {
                if (registration.Activator.LimitType == typeof(ProxDependency)) {
                    registration.InterceptedBy<ProxIntercept>();
                }
            }
        }

        [Test]
        public void DynamicProxyAndShellSettingsAreResolvableToSameInstances() {
            var topology = CreateTopology();

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(topology);

            var proxa = shellContainer.Resolve<DynamicProxyContext>();
            var proxb = shellContainer.Resolve<DynamicProxyContext>();
            var setta = shellContainer.Resolve<ShellSettings>();
            var settb = shellContainer.Resolve<ShellSettings>();

            Assert.That(proxa, Is.Not.Null);
            Assert.That(proxa, Is.SameAs(proxb));
            Assert.That(setta, Is.Not.Null);
            Assert.That(setta, Is.SameAs(settb));


            var topology2 = CreateTopology();
            var shellContainer2 = factory.CreateContainer(topology2);

            var proxa2 = shellContainer2.Resolve<DynamicProxyContext>();
            var proxb2 = shellContainer2.Resolve<DynamicProxyContext>();
            var setta2 = shellContainer2.Resolve<ShellSettings>();
            var settb2 = shellContainer2.Resolve<ShellSettings>();

            Assert.That(proxa2, Is.Not.Null);
            Assert.That(proxa2, Is.SameAs(proxb2));
            Assert.That(setta2, Is.Not.Null);
            Assert.That(setta2, Is.SameAs(settb2));

            Assert.That(proxa, Is.Not.SameAs(proxa2));
            Assert.That(setta, Is.Not.SameAs(setta2));
        }
    }
}
