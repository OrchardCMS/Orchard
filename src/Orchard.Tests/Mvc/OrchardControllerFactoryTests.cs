using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Orchard.Mvc;
using Orchard.Tests.Stubs;
using Autofac;

namespace Orchard.Tests.Mvc {
    [TestFixture]
    public class OrchardControllerFactoryTests {
        private OrchardControllerFactory _controllerFactory;
        private StubContainerProvider _containerProvider;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<ReplacementFooController>()
                .Keyed<IController>("/foo")
                .InstancePerDependency();

            var container = builder.Build();
            _containerProvider = new StubContainerProvider(container, container.BeginLifetimeScope());


            _controllerFactory = new OrchardControllerFactory();
            InjectKnownControllerTypes(_controllerFactory, typeof (FooController), typeof (BarController));
        }

        [Test]
        public void IContainerProvidersRequestContainerFromRouteDataShouldUseTokenWhenPresent() {
            var requestContext = GetRequestContext(_containerProvider);
            var controller = _controllerFactory.CreateController(requestContext, "foo");

            Assert.That(controller, Is.TypeOf<ReplacementFooController>());
        }

        [Test]
        public void WhenNullOrMissingContainerNormalControllerFactoryRulesShouldBeUsedAsFallback() {
            var requestContext = GetRequestContext(null);
            var controller = _controllerFactory.CreateController(requestContext, "foo");

            Assert.That(controller, Is.TypeOf<FooController>());
        }

        [Test]
        public void
            WhenContainerIsPresentButNamedControllerIsNotResolvedNormalControllerFactoryRulesShouldBeUsedAsFallback() {
            var requestContext = GetRequestContext(_containerProvider);
            var controller = _controllerFactory.CreateController(requestContext, "bar");

            Assert.That(controller, Is.TypeOf<BarController>());
        }

        [Test]
        public void DisposingControllerThatCameFromContainerShouldNotCauseProblemWhenContainerIsDisposed() {
            var requestContext = GetRequestContext(_containerProvider);
            var controller = _controllerFactory.CreateController(requestContext, "foo");

            Assert.That(controller, Is.TypeOf<ReplacementFooController>());

            _controllerFactory.ReleaseController(controller);
            _containerProvider.EndRequestLifetime();

            // explicitly dispose a few more, just to make sure it's getting hit from all different directions
            ((IDisposable) controller).Dispose();
            ((ReplacementFooController) controller).Dispose();

            Assert.That(((ReplacementFooController) controller).Disposals, Is.EqualTo(4));
        }

        private static RequestContext GetRequestContext(StubContainerProvider containerProvider) {
            var handler = new MvcRouteHandler();
            var route = new Route("yadda", handler) {
                                                        DataTokens =
                                                            new RouteValueDictionary
                                                            {{"IContainerProvider", containerProvider}}
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