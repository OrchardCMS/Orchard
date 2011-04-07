using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Orchard.Environment.Extensions.Models;
using Orchard.Mvc;
using Orchard.Tests.DisplayManagement;
using Orchard.Tests.Stubs;
using Autofac;

namespace Orchard.Tests.Mvc {
    [TestFixture]
    public class OrchardControllerFactoryTests {
        private OrchardControllerFactory _controllerFactory;
        private IWorkContextAccessor _workContextAccessor;
        private StubContainerProvider _containerProvider;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<FooController>()
                .Keyed<IController>("/foo")
                .Keyed<IController>(typeof(FooController))
                .WithMetadata("ControllerType", typeof(FooController))
                .InstancePerDependency();

            builder.RegisterType<BarController>()
                .Keyed<IController>("/bar")
                .Keyed<IController>(typeof(BarController))
                .WithMetadata("ControllerType", typeof(BarController))
                .InstancePerDependency();

            builder.RegisterType<ReplacementFooController>()
                .Keyed<IController>("/foo")
                .Keyed<IController>(typeof(ReplacementFooController))
                .WithMetadata("ControllerType", typeof(ReplacementFooController))
                .InstancePerDependency();

            var container = builder.Build();
            _containerProvider = new StubContainerProvider(container, container.BeginLifetimeScope());

            var workContext = new DefaultDisplayManagerTests.TestWorkContext
            {
                CurrentTheme = new ExtensionDescriptor { Id = "Hello" },
                ContainerProvider = _containerProvider
            };
            _workContextAccessor = new DefaultDisplayManagerTests.TestWorkContextAccessor(workContext);

            _controllerFactory = new OrchardControllerFactory();
            InjectKnownControllerTypes(_controllerFactory, typeof(ReplacementFooController), typeof (FooController), typeof (BarController));
        }

        [Test]
        public void IContainerProvidersRequestContainerFromRouteDataShouldUseTokenWhenPresent() {
            var requestContext = GetRequestContext(_workContextAccessor);
            var controller = _controllerFactory.CreateController(requestContext, "foo");

            Assert.That(controller, Is.TypeOf<ReplacementFooController>());
        }

        [Test, Ignore("OrchardControllerFactory depends on metadata, calling base when no context is causing errors.")]
        public void WhenNullOrMissingContainerNormalControllerFactoryRulesShouldBeUsedAsFallback() {
            var requestContext = GetRequestContext(null);
            var controller = _controllerFactory.CreateController(requestContext, "foo");

            Assert.That(controller, Is.TypeOf<FooController>());
        }

        [Test]
        public void WhenContainerIsPresentButNamedControllerIsNotResolvedNormalControllerFactoryRulesShouldBeUsedAsFallback() {
            var requestContext = GetRequestContext(_workContextAccessor);
            var controller = _controllerFactory.CreateController(requestContext, "bar");

            Assert.That(controller, Is.TypeOf<BarController>());
        }

        [Test]
        public void DisposingControllerThatCameFromContainerShouldNotCauseProblemWhenContainerIsDisposed() {
            var requestContext = GetRequestContext(_workContextAccessor);
            var controller = _controllerFactory.CreateController(requestContext, "foo");

            Assert.That(controller, Is.TypeOf<ReplacementFooController>());

            _controllerFactory.ReleaseController(controller);
            _containerProvider.EndRequestLifetime();

            // explicitly dispose a few more, just to make sure it's getting hit from all different directions
            ((IDisposable) controller).Dispose();
            ((ReplacementFooController) controller).Dispose();

            Assert.That(((ReplacementFooController) controller).Disposals, Is.EqualTo(4));
        }

        [Test]
        public void NullServiceKeyReturnsDefault() {
            OrchardControllerFactoryAccessor orchardControllerFactory = new OrchardControllerFactoryAccessor();
            ReplacementFooController fooController;

            Assert.That(orchardControllerFactory.TryResolveAccessor(_workContextAccessor.GetContext(), null, out fooController), Is.False);
            Assert.That(fooController, Is.Null);
        }

        private static RequestContext GetRequestContext(IWorkContextAccessor workContextAccessor)
        {
            var handler = new MvcRouteHandler();
            var route = new Route("yadda", handler) {
                                                        DataTokens =
                                                            new RouteValueDictionary { { "IWorkContextAccessor", workContextAccessor } }
                                                    };

            var httpContext = new StubHttpContext();
            var routeData = route.GetRouteData(httpContext);
            return new RequestContext(httpContext, routeData);
        }

        public class FooController : Controller { }

        public class BarController : Controller { }

        public class ReplacementFooController : Controller {
            protected override void Dispose(bool disposing) {
                ++Disposals;

                base.Dispose(disposing);
            }

            public int Disposals { get; set; }
        }

        internal class OrchardControllerFactoryAccessor : OrchardControllerFactory {
            public bool TryResolveAccessor<T>(WorkContext workContext, object serviceKey, out T instance) {
                return TryResolve(workContext, serviceKey, out instance);
            }
        }

        private static void InjectKnownControllerTypes(DefaultControllerFactory controllerFactory,
                                                       params Type[] controllerTypes) {
            // D'oh!!! Hey MVC people, how is this testable? ;)

            // locate the appropriate reflection member info
            var controllerTypeCacheProperty = controllerFactory.GetType()
                .GetProperty("ControllerTypeCache", BindingFlags.Instance | BindingFlags.NonPublic);
            var cacheField = controllerTypeCacheProperty.PropertyType.GetField("_cache",
                                                                               BindingFlags.NonPublic |
                                                                               BindingFlags.Instance);

            // turn the array into the correct collection
            var cache = controllerTypes
                .GroupBy(t => t.Name.Substring(0, t.Name.Length - "Controller".Length), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key,
                              g => g.ToLookup(t => t.Namespace ?? string.Empty, StringComparer.OrdinalIgnoreCase),
                              StringComparer.OrdinalIgnoreCase);

            // execute: controllerFactory.ControllerTypeCache._cache = cache;
            cacheField.SetValue(
                controllerTypeCacheProperty.GetValue(controllerFactory, null),
                cache);
        }
   }
}