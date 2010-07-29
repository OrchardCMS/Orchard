using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using PackageIndexReferenceImplementation.Models;

namespace PackageIndexReferenceImplementation.Controllers {

    [HandleError]
    public class AccountController : Controller {

        public IFormsAuthenticationService FormsService { get; set; }

        protected override void Initialize(RequestContext requestContext) {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }

            base.Initialize(requestContext);
        }

        // **************************************
        // URL: /Account/LogOn
        // **************************************

        public ActionResult LogOn() {
            return View();
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl) {
            if (ModelState.IsValid) {
                if ( FormsAuthentication.Authenticate(model.UserName, model.Password) ) {
                    FormsService.SignIn(model.UserName, model.RememberMe);
                    if (!String.IsNullOrEmpty(returnUrl)) {
                        return Redirect(returnUrl);
                    }
                    else {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // **************************************
        // URL: /Account/LogOff
        // **************************************

        public ActionResult LogOff() {
            FormsService.SignOut();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult SHA1(string password) {
            return new ContentResult { Content = FormsAuthentication.HashPasswordForStoringInConfigFile(password, "sha1") };
        }
    }
}
