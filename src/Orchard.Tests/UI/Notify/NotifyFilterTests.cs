using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.UI.Notify {
    [TestFixture]
    public class NotifyFilterTests {
        private static ActionExecutedContext BuildContext() {
            var httpContext = new StubHttpContext();
            var routeData = new RouteData();

            var controllerContext = new ControllerContext(httpContext, routeData, new Mock<ControllerBase>().Object);
            var actionDescriptor = new Mock<ActionDescriptor>().Object;
            return new ActionExecutedContext(controllerContext, actionDescriptor, false/*cancelled*/, null/*exception*/);
        }
    }
}