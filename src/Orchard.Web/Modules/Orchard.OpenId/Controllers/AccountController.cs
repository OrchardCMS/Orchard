﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.OpenId.Services;
using Orchard.Security;
using Orchard.Themes;
using Orchard.Users.Events;

namespace Orchard.OpenId.Controllers
{
    [Themed]
    [OrchardFeature("Orchard.OpenId")]
    public class AccountController : Controller {
        private readonly IEnumerable<IOpenIdProvider> _openIdProviders;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;
        private readonly IOrchardServices _orchardServices;
        private readonly IUserEventHandler _userEventHandler;

        public AccountController(
            IEnumerable<IOpenIdProvider> openIdProviders,
            IAuthenticationService authenticationService,
            IMembershipService membershipService,
            IOrchardServices orchardServices,
            IUserEventHandler userEventHandler) {

            _openIdProviders = openIdProviders;
            _authenticationService = authenticationService;
            _membershipService = membershipService;
            _orchardServices = orchardServices;
            _userEventHandler = userEventHandler;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        [HttpGet]
        public ActionResult LogOn() {
            if (Request.IsAuthenticated) {
                return Redirect(Url.Content("~/"));
            }

            return View(_openIdProviders);
        }

        [HttpPost]
        [AlwaysAccessible]
        [ValidateInput(false)]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Needs to take same parameter type as Controller.Redirect()")]
        public ActionResult LogOn(string userNameOrEmail, string password, string returnUrl, bool rememberMe = false) {
            _userEventHandler.LoggingIn(userNameOrEmail, password);

            var user = ValidateLogOn(userNameOrEmail, password);
            if (!ModelState.IsValid) {
                return View(_openIdProviders);
            }

            var membershipSettings = _membershipService.GetSettings();
            if (user != null &&
                membershipSettings.EnableCustomPasswordPolicy &&
                membershipSettings.EnablePasswordExpiration &&
                _membershipService.PasswordIsExpired(user, membershipSettings.PasswordExpirationTimeInDays)) {
                return RedirectToAction("ChangeExpiredPassword", new { username = user.UserName });
            }

            _authenticationService.SignIn(user, rememberMe);
            _userEventHandler.LoggedIn(user);

            return this.RedirectLocal(returnUrl);
        }

        public void Challenge(string openIdProvider) {
            _userEventHandler.LoggingIn(openIdProvider, String.Empty);

            if (String.IsNullOrWhiteSpace(openIdProvider))
                openIdProvider = OpenIdConnectAuthenticationDefaults.AuthenticationType;

            if (Request.IsAuthenticated) {
                Redirect(Url.Content("~/"));
                return;
            }

            var redirectUri = Url.Content(String.Concat(Constants.General.LogonCallbackUrl));

            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = redirectUri }, openIdProvider);
        }

        public RedirectResult LogOff(string openIdProvider) {
            if (String.IsNullOrWhiteSpace(openIdProvider))
                openIdProvider = OpenIdConnectAuthenticationDefaults.AuthenticationType;

            HttpContext.GetOwinContext().Authentication.SignOut(openIdProvider, CookieAuthenticationDefaults.AuthenticationType);
            _authenticationService.SignOut();

            var loggedUser = _authenticationService.GetAuthenticatedUser();
            if (loggedUser != null) {
                _userEventHandler.LoggedOut(loggedUser);
            }

            return Redirect(Url.Content("~/"));
        }

        public RedirectResult LogonCallback() {
            var user = _authenticationService.GetAuthenticatedUser();
            _userEventHandler.LoggedIn(user);

            return Redirect(Url.Content("~/"));
        }

        public ActionResult AccessDenied() {
            var returnUrl = Request.QueryString["ReturnUrl"];
            var currentUser = _authenticationService.GetAuthenticatedUser();

            if (currentUser == null) {
                return RedirectToAction("Logon");
            }

            _userEventHandler.AccessDenied(currentUser);

            return View();
        }

        public ActionResult Error() {
            return View();
        }

        private IUser ValidateLogOn(string userNameOrEmail, string password) {
            bool validate = true;

            if (String.IsNullOrEmpty(userNameOrEmail)) {
                ModelState.AddModelError("userNameOrEmail", T("You must specify a username or e-mail."));
                validate = false;
            }
            if (String.IsNullOrEmpty(password)) {
                ModelState.AddModelError("password", T("You must specify a password."));
                validate = false;
            }

            if (!validate)
                return null;

            var user = _membershipService.ValidateUser(userNameOrEmail, password);
            if (user == null) {
                ModelState.AddModelError("password", T("The username or e-mail or password provided is incorrect."));
            }

            return user;
        }
    }
}