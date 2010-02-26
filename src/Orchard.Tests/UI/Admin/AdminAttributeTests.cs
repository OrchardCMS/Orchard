using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.UI.Admin;

namespace Orchard.Tests.UI.Admin {
    [TestFixture]
    public class AdminAttributeTests {

        private static AuthorizationContext GetAuthorizationContext<TController>() {
            var controllerDescriptor = new ReflectedControllerDescriptor(typeof(TController));
            var controllerContext = new ControllerContext();
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

            var filter = new AdminAuthorizationFilter(GetAuthorizer(false));
            filter.OnAuthorization(authorizationContext);

            Assert.That(authorizationContext.Result, Is.Null);
        }

        [Test]
        public void AdminRequestShouldRequirePermission() {
            var authorizationContext = GetAuthorizationContext<AdminController>();
            var filter = new AdminAuthorizationFilter(GetAuthorizer(false));
            filter.OnAuthorization(authorizationContext);
            Assert.That(authorizationContext.Result, Is.InstanceOf<HttpUnauthorizedResult>());

            var authorizationContext2 = GetAuthorizationContext<AdminController>();
            var filter2 = new AdminAuthorizationFilter(GetAuthorizer(true));
            filter2.OnAuthorization(authorizationContext2);
            Assert.That(authorizationContext2.Result, Is.Null);
        }

        [Test]
        public void NormalWithAttribRequestShouldRequirePermission() {
            var authorizationContext = GetAuthorizationContext<NormalWithAttribController>();
            var filter = new AdminAuthorizationFilter(GetAuthorizer(false));
            filter.OnAuthorization(authorizationContext);
            Assert.That(authorizationContext.Result, Is.InstanceOf<HttpUnauthorizedResult>());

            var authorizationContext2 = GetAuthorizationContext<NormalWithAttribController>();
            var filter2 = new AdminAuthorizationFilter(GetAuthorizer(true));
            filter2.OnAuthorization(authorizationContext2);
            Assert.That(authorizationContext2.Result, Is.Null);
        }
        
        [Test]
        public void NormalWithActionAttribRequestShouldRequirePermission() {
            var authorizationContext = GetAuthorizationContext<NormalWithActionAttribController>();
            var filter = new AdminAuthorizationFilter(GetAuthorizer(false));
            filter.OnAuthorization(authorizationContext);
            Assert.That(authorizationContext.Result, Is.InstanceOf<HttpUnauthorizedResult>());

            var authorizationContext2 = GetAuthorizationContext<NormalWithActionAttribController>();
            var filter2 = new AdminAuthorizationFilter(GetAuthorizer(true));
            filter2.OnAuthorization(authorizationContext2);
            Assert.That(authorizationContext2.Result, Is.Null);
        }

        [Test]
        public void InheritedAttribRequestShouldRequirePermission() {
            var authorizationContext = GetAuthorizationContext<InheritedAttribController>();
            var filter = new AdminAuthorizationFilter(GetAuthorizer(false));
            filter.OnAuthorization(authorizationContext);
            Assert.That(authorizationContext.Result, Is.InstanceOf<HttpUnauthorizedResult>());

            var authorizationContext2 = GetAuthorizationContext<InheritedAttribController>();
            var filter2 = new AdminAuthorizationFilter(GetAuthorizer(true));
            filter2.OnAuthorization(authorizationContext2);
            Assert.That(authorizationContext2.Result, Is.Null);
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
