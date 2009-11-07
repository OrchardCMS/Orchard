using System.Web.Routing;
using NUnit.Framework;
using Orchard.Web.Tests.Stubs;

namespace Orchard.Web.Tests.Routes {
    [TestFixture]
    public class RouteTests {
        [Test]
        public void RouteForEmbeddedResource() {
            // Arrange
            var context = new StubContext("~/foo.axd/bar/baz/biff");
            var routes = new RouteCollection();
            MvcApplication.RegisterRoutes(routes);

            // Act
            var routeData = routes.GetRouteData(context);

            // Assert
            Assert.That(routeData, Is.Not.Null);
            Assert.That(routeData.RouteHandler, Is.TypeOf<StopRoutingHandler>());
        }

        [Test]
        public void RouteWithControllerNoActionNoId() {
            // Arrange
            var context = new StubContext("~/controller1");
            var routes = new RouteCollection();
            MvcApplication.RegisterRoutes(routes);

            // Act
            var routeData = routes.GetRouteData(context);

            // Assert
            Assert.That(routeData, Is.Not.Null);
            Assert.That(routeData.Values["controller"], Is.EqualTo("controller1"));
            Assert.That(routeData.Values["action"], Is.EqualTo("Index"));
            Assert.That(routeData.Values["id"], Is.EqualTo(""));
        }

        [Test]
        public void RouteWithControllerWithActionNoId() {
            // Arrange
            var context = new StubContext("~/controller1/action2");
            var routes = new RouteCollection();
            MvcApplication.RegisterRoutes(routes);

            // Act
            var routeData = routes.GetRouteData(context);

            // Assert
            Assert.That(routeData, Is.Not.Null);
            Assert.That(routeData.Values["controller"], Is.EqualTo("controller1"));
            Assert.That(routeData.Values["action"], Is.EqualTo("action2"));
            Assert.That(routeData.Values["id"], Is.EqualTo(""));
        }

        [Test]
        public void RouteWithControllerWithActionWithId() {
            // Arrange
            var context = new StubContext("~/controller1/action2/id3");
            var routes = new RouteCollection();
            MvcApplication.RegisterRoutes(routes);

            // Act
            var routeData = routes.GetRouteData(context);

            // Assert
            Assert.That(routeData, Is.Not.Null);
            Assert.That(routeData.Values["controller"], Is.EqualTo("controller1"));
            Assert.That(routeData.Values["action"], Is.EqualTo("action2"));
            Assert.That(routeData.Values["id"], Is.EqualTo("id3"));
        }

        [Test]
        public void RouteWithTooManySegments() {
            // Arrange
            var context = new StubContext("~/a/b/c/d");
            var routes = new RouteCollection();
            MvcApplication.RegisterRoutes(routes);

            // Act
            var routeData = routes.GetRouteData(context);

            // Assert
            Assert.That(routeData, Is.Null);
        }
    }
}