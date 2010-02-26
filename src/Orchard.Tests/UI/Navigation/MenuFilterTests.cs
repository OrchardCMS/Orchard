using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Orchard.Localization;
using Orchard.Mvc.ViewModels;
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


        [Test]
        public void MainMenuShouldBeCalledNormally() {
            Mock<INavigationManager> navigationManager = GetNavigationManager();

            var authorizationContext = GetAuthorizationContext<NormalController>();
            var adminFilter = new AdminFilter(GetAuthorizer(true));
            adminFilter.OnAuthorization(authorizationContext);

            var viewModel = new BaseViewModel();
            var resultExecutingContext = new ResultExecutingContext(
                authorizationContext,
                new ViewResult { ViewData = new ViewDataDictionary<BaseViewModel>(viewModel) });
            var menuFilter = new MenuFilter(navigationManager.Object);
            menuFilter.OnResultExecuting(resultExecutingContext);

            Assert.That(viewModel.Menu, Is.Not.Null);
            Assert.That(viewModel.Menu.Single().Text, Is.SameAs("The Main Menu"));
        }

        [Test]
        public void AdminMenuShouldHaveDifferentNavigation() {
            Mock<INavigationManager> navigationManager = GetNavigationManager();

            var authorizationContext = GetAuthorizationContext<AdminController>();
            var adminFilter = new AdminFilter(GetAuthorizer(true));
            adminFilter.OnAuthorization(authorizationContext);

            var viewModel = new BaseViewModel();
            var resultExecutingContext = new ResultExecutingContext(
                authorizationContext,
                new ViewResult { ViewData = new ViewDataDictionary<BaseViewModel>(viewModel) });
            var menuFilter = new MenuFilter(navigationManager.Object);
            menuFilter.OnResultExecuting(resultExecutingContext);

            Assert.That(viewModel.Menu, Is.Not.Null);
            Assert.That(viewModel.Menu.Single().Text, Is.SameAs("The Admin Menu"));
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
