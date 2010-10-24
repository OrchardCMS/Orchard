using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Tests.Stubs;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;

namespace Orchard.Tests.UI.Navigation {
    [TestFixture]
    public class MenuFilterTests {

        private static AuthorizationContext GetAuthorizationContext<TController>() where TController : ControllerBase, new() {
            var controllerDescriptor = new ReflectedControllerDescriptor(typeof(TController));
            var controllerContext = new ControllerContext(new StubHttpContext(), new RouteData(), new TController());
            return new AuthorizationContext(
                controllerContext,
                controllerDescriptor.FindAction(controllerContext, "Index"));
        }

        private static IAuthorizer GetAuthorizer(bool result) {
            var authorizer = new Mock<IAuthorizer>();
            authorizer
                .Setup(x => x.Authorize(StandardPermissions.AccessAdminPanel, It.IsAny<LocalizedString>())).
                Returns(result);
            return authorizer.Object;
        }

        private static Mock<INavigationManager> GetNavigationManager() {
            var mainMenu = new[] { new MenuItem { Text = "The Main Menu" } };
            var adminMenu = new[] { new MenuItem { Text = "The Admin Menu" } };
            var navigationManager = new Mock<INavigationManager>();
            navigationManager.Setup(x => x.BuildMenu("main")).Returns(mainMenu);
            navigationManager.Setup(x => x.BuildMenu("admin")).Returns(adminMenu);
            return navigationManager;
        }

        [Test]
        public void MockNavManagerWorks() {
            var main = GetNavigationManager().Object.BuildMenu("main");
            Assert.That(main.Count(), Is.EqualTo(1));
        }
    }


    public class NormalController : Controller {
        public ActionResult Index() {
            return View();
        }
    }

    public class AdminController : Controller {
        public ActionResult Index() {
            return View();
        }
    }
}
