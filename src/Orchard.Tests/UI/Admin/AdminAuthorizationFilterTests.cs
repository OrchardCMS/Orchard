using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Tests.Stubs;
using Orchard.UI.Admin;

namespace Orchard.Tests.UI.Admin {
    [TestFixture]
    public class AdminAuthorizationFilterTests {

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

        [Test]
        public void NormalRequestShouldNotBeAffected() {
            var authorizationContext = GetAuthorizationContext<NormalController>();

            var filter = new AdminFilter(GetAuthorizer(false));
            filter.OnAuthorization(authorizationContext);

            Assert.That(authorizationContext.Result, Is.Null);
        }

        private static void TestActionThatShouldRequirePermission<TController>() where TController : ControllerBase, new() {
            var authorizationContext = GetAuthorizationContext<TController>();
            var filter = new AdminFilter(GetAuthorizer(false));
            filter.OnAuthorization(authorizationContext);
            Assert.That(authorizationContext.Result, Is.InstanceOf<HttpUnauthorizedResult>());
            Assert.That(AdminFilter.IsApplied(authorizationContext.RequestContext), Is.True);

            var authorizationContext2 = GetAuthorizationContext<TController>();
            var filter2 = new AdminFilter(GetAuthorizer(true));
            filter2.OnAuthorization(authorizationContext2);
            Assert.That(authorizationContext2.Result, Is.Null);
            Assert.That(AdminFilter.IsApplied(authorizationContext2.RequestContext), Is.True);
        }


        [Test]
        public void AdminRequestShouldRequirePermission() {
            TestActionThatShouldRequirePermission<AdminController>();
        }

        [Test]
        public void NormalWithAttribRequestShouldRequirePermission() {
            TestActionThatShouldRequirePermission<NormalWithAttribController>();
        }

        [Test]
        public void NormalWithActionAttribRequestShouldRequirePermission() {
            TestActionThatShouldRequirePermission<NormalWithActionAttribController>();
        }

        [Test]
        public void InheritedAttribRequestShouldRequirePermission() {
            TestActionThatShouldRequirePermission<InheritedAttribController>();
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

    [Admin]
    public class NormalWithAttribController : Controller {
        public ActionResult Index() {
            return View();
        }
    }

    public class NormalWithActionAttribController : Controller {
        [Admin]
        public ActionResult Index() {
            return View();
        }
    }

    [Admin]
    public class BaseWithAttribController : Controller {
        public ActionResult Something() {
            return View();
        }
    }

    public class InheritedAttribController : BaseWithAttribController {
        public ActionResult Index() {
            return View();
        }
    }
}
