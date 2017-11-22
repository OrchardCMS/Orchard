using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Utility.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Security;
using Orchard.Services;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Users.Controllers {
    [HandleError, Themed]
    public class AccountController : Controller {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;
        private readonly IUserService _userService;
        private readonly IOrchardServices _orchardServices;
        private readonly IUserEventHandler _userEventHandler;
        private readonly IClock _clock;

        public AccountController(
            IAuthenticationService authenticationService,
            IMembershipService membershipService,
            IUserService userService,
            IOrchardServices orchardServices,
            IUserEventHandler userEventHandler,
            IClock clock) {

            _authenticationService = authenticationService;
            _membershipService = membershipService;
            _userService = userService;
            _orchardServices = orchardServices;
            _userEventHandler = userEventHandler;
            _clock = clock;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        [AlwaysAccessible]
        public ActionResult AccessDenied() {
            var returnUrl = Request.QueryString["ReturnUrl"];
            var currentUser = _authenticationService.GetAuthenticatedUser();

            if (currentUser == null) {
                Logger.Information("Access denied to anonymous request on {0}", returnUrl);
                var shape = _orchardServices.New.LogOn().Title(T("Access Denied").Text);
                return new ShapeResult(this, shape);
            }

            //TODO: (erikpo) Add a setting for whether or not to log access denieds since these can fill up a database pretty fast from bots on a high traffic site
            //Suggestion: Could instead use the new AccessDenined IUserEventHandler method and let modules decide if they want to log this event?
            Logger.Information("Access denied to user #{0} '{1}' on {2}", currentUser.Id, currentUser.UserName, returnUrl);

            _userEventHandler.AccessDenied(currentUser);

            return View();
        }

        [AlwaysAccessible]
        public ActionResult LogOn(string returnUrl) {
            if (_authenticationService.GetAuthenticatedUser() != null)
                return this.RedirectLocal(returnUrl);

            var shape = _orchardServices.New.LogOn().Title(T("Log On").Text);
            return new ShapeResult(this, shape);
        }

        [HttpPost]
        [AlwaysAccessible]
        [ValidateInput(false)]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
            Justification = "Needs to take same parameter type as Controller.Redirect()")]
        public ActionResult LogOn(string userNameOrEmail, string password, string returnUrl, bool rememberMe = false) {
            _userEventHandler.LoggingIn(userNameOrEmail, password);

            var user = ValidateLogOn(userNameOrEmail, password);
            if (!ModelState.IsValid) {
                var shape = _orchardServices.New.LogOn().Title(T("Log On").Text);

                return new ShapeResult(this, shape);
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

        public ActionResult LogOff(string returnUrl) {
            _authenticationService.SignOut();

            var loggedUser = _authenticationService.GetAuthenticatedUser();
            if (loggedUser != null) {
                _userEventHandler.LoggedOut(loggedUser);
            }

            return this.RedirectLocal(returnUrl);
        }

        [AlwaysAccessible]
        public ActionResult Register() {
            // ensure users can register
            var membershipSettings = _membershipService.GetSettings();
            if (!membershipSettings.UsersCanRegister) {
                return HttpNotFound();
            }

            ViewData["PasswordLength"] = membershipSettings.GetMinimumPasswordLength();

            var shape = _orchardServices.New.Register();

            return new ShapeResult(this, shape);
        }

        [HttpPost]
        [AlwaysAccessible]
        [ValidateInput(false)]
        public ActionResult Register(string userName, string email, string password, string confirmPassword, string returnUrl = null) {
            // ensure users can register
            var membershipSettings = _membershipService.GetSettings();
            if (!membershipSettings.UsersCanRegister) {
                return HttpNotFound();
            }

            ViewData["PasswordLength"] = membershipSettings.GetMinimumPasswordLength();

            if (ValidateRegistration(userName, email, password, confirmPassword)) {
                // Attempt to register the user
                // No need to report this to IUserEventHandler because _membershipService does that for us
                var user = _membershipService.CreateUser(new CreateUserParams(userName, password, email, null, null, false));

                if (user != null) {
                    if (user.As<UserPart>().EmailStatus == UserStatus.Pending) {
                        var siteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;
                        if (String.IsNullOrWhiteSpace(siteUrl)) {
                            siteUrl = HttpContext.Request.ToRootUrlString();
                        }

                        _userService.SendChallengeEmail(user.As<UserPart>(), nonce => Url.MakeAbsolute(Url.Action("ChallengeEmail", "Account", new { Area = "Orchard.Users", nonce = nonce }), siteUrl));

                        _userEventHandler.SentChallengeEmail(user);
                        return RedirectToAction("ChallengeEmailSent", new { ReturnUrl = returnUrl });
                    }

                    if (user.As<UserPart>().RegistrationStatus == UserStatus.Pending) {
                        return RedirectToAction("RegistrationPending", new { ReturnUrl = returnUrl });
                    }

                    _userEventHandler.LoggingIn(userName, password);
                    _authenticationService.SignIn(user, false /* createPersistentCookie */);
                    _userEventHandler.LoggedIn(user);

                    return this.RedirectLocal(returnUrl);
                }

                ModelState.AddModelError("_FORM", T(ErrorCodeToString(/*createStatus*/MembershipCreateStatus.ProviderError)));
            }

            // If we got this far, something failed, redisplay form
            var shape = _orchardServices.New.Register();
            return new ShapeResult(this, shape);
        }

        [AlwaysAccessible]
        public ActionResult RequestLostPassword() {
            // ensure users can request lost password
            var membershipSettings = _membershipService.GetSettings();
            if (!membershipSettings.EnableLostPassword) {
                return HttpNotFound();
            }

            return View();
        }

        [HttpPost]
        [AlwaysAccessible]
        public ActionResult RequestLostPassword(string username) {
            // ensure users can request lost password
            var membershipSettings = _membershipService.GetSettings();
            if (!membershipSettings.EnableLostPassword) {
                return HttpNotFound();
            }

            if (String.IsNullOrWhiteSpace(username)) {
                ModelState.AddModelError("username", T("You must specify a username or e-mail."));
                return View();
            }

            var siteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;
            if (String.IsNullOrWhiteSpace(siteUrl)) {
                siteUrl = HttpContext.Request.ToRootUrlString();
            }

            _userService.SendLostPasswordEmail(username, nonce => Url.MakeAbsolute(Url.Action("LostPassword", "Account", new { Area = "Orchard.Users", nonce = nonce }), siteUrl));

            _orchardServices.Notifier.Information(T("Check your e-mail for the confirmation link."));

            return RedirectToAction("LogOn");
        }

        [Authorize]
        [AlwaysAccessible]
        public ActionResult ChangePassword() {
            var membershipSettings = _membershipService.GetSettings();
            ViewData["PasswordLength"] = membershipSettings.GetMinimumPasswordLength();

            return View();
        }

        [Authorize]
        [HttpPost]
        [AlwaysAccessible]
        [ValidateInput(false)]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Exceptions result in password not being changed.")]
        public ActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword) {
            var membershipSettings = _membershipService.GetSettings();
            ViewData["PasswordLength"] = membershipSettings.GetMinimumPasswordLength(); ;

            if (!ValidateChangePassword(currentPassword, newPassword, confirmPassword)) {
                return View();
            }

            if (PasswordChangeIsSuccess(currentPassword, newPassword, _orchardServices.WorkContext.CurrentUser.UserName)) {
                return RedirectToAction("ChangePasswordSuccess");
            }
            else {
                return ChangePassword();
            }
        }

        [AlwaysAccessible]
        public ActionResult ChangeExpiredPassword(string username) {
            var membershipSettings = _membershipService.GetSettings();
            var lastPasswordChangeUtc = _membershipService.GetUser(username).As<UserPart>().LastPasswordChangeUtc;

            if (lastPasswordChangeUtc.Value.AddDays(membershipSettings.PasswordExpirationTimeInDays) >
                _clock.UtcNow) {
                return RedirectToAction("LogOn");
            }

            var viewModel = _orchardServices.New.ViewModel(
                Username: username,
                PasswordLength: membershipSettings.GetMinimumPasswordLength());

            return View(viewModel);
        }

        [HttpPost, AlwaysAccessible, ValidateInput(false)]
        public ActionResult ChangeExpiredPassword(string currentPassword, string newPassword, string confirmPassword, string username) {
            var membershipSettings = _membershipService.GetSettings();
            var viewModel = _orchardServices.New.ViewModel(
                Username: username,
                PasswordLength: membershipSettings.GetMinimumPasswordLength());

            if (!ValidateChangePassword(currentPassword, newPassword, confirmPassword)) {
                return View(viewModel);
            }

            if (PasswordChangeIsSuccess(currentPassword, newPassword, username)) {
                return RedirectToAction("ChangePasswordSuccess");
            }
            else {
                return View(viewModel);
            }
        }

        private bool PasswordChangeIsSuccess(string currentPassword, string newPassword, string username) {
            try {
                List<LocalizedString> validationErrors;
                var validated = _membershipService.ValidateUser(username, currentPassword, out validationErrors);

                if (validated != null) {
                    _membershipService.SetPassword(validated, newPassword);
                    _userEventHandler.ChangedPassword(validated);

                    return true;
                }

                ModelState.AddModelError("_FORM", T("The current password is incorrect or the new password is invalid."));

                return false;
            }
            catch {
                ModelState.AddModelError("_FORM", T("The current password is incorrect or the new password is invalid."));

                return false;
            }
        }

        [AlwaysAccessible]
        public ActionResult LostPassword(string nonce) {
            if ( _userService.ValidateLostPassword(nonce) == null) {
                return RedirectToAction("LogOn");
            }

            var membershipSettings = _membershipService.GetSettings();
            ViewData["PasswordLength"] = membershipSettings.GetMinimumPasswordLength();

            return View();
        }

        [HttpPost]
        [AlwaysAccessible]
        [ValidateInput(false)]
        public ActionResult LostPassword(string nonce, string newPassword, string confirmPassword) {
            IUser user;
            if ( (user = _userService.ValidateLostPassword(nonce)) == null) {
                return Redirect("~/");
            }

            var membershipSettings = _membershipService.GetSettings();
            ViewData["PasswordLength"] = membershipSettings.GetMinimumPasswordLength();

            ValidatePassword(newPassword);

            if (!String.Equals(newPassword, confirmPassword, StringComparison.Ordinal)) {
                ModelState.AddModelError("_FORM", T("The new password and confirmation password do not match."));
            }

            if (!ModelState.IsValid) {
                return View();
            }

            _membershipService.SetPassword(user, newPassword);

            _userEventHandler.ChangedPassword(user);

            return RedirectToAction("ChangePasswordSuccess");
        }

        [AlwaysAccessible]
        public ActionResult ChangePasswordSuccess() {
            return View();
        }

        public ActionResult RegistrationPending() {
            return View();
        }

        public ActionResult ChallengeEmailSent() {
            return View();
        }

        public ActionResult ChallengeEmailSuccess() {
            return View();
        }

        public ActionResult ChallengeEmailFail() {
            return View();
        }

        public ActionResult ChallengeEmail(string nonce) {
            var user = _userService.ValidateChallenge(nonce);

            if ( user != null) {
                _userEventHandler.ConfirmedEmail(user);

                return RedirectToAction("ChallengeEmailSuccess");
            }

            return RedirectToAction("ChallengeEmailFail");
        }

        #region Validation Methods
        private bool ValidateChangePassword(string currentPassword, string newPassword, string confirmPassword) {
            if ( String.IsNullOrEmpty(currentPassword)) {
                ModelState.AddModelError("currentPassword", T("You must specify a current password."));
            }

            if (String.Equals(currentPassword, newPassword, StringComparison.Ordinal)) {
                ModelState.AddModelError("newPassword", T("The new password must be different from the current password."));
            }

            ValidatePassword(newPassword);

            if ( !String.Equals(newPassword, confirmPassword, StringComparison.Ordinal)) {
                ModelState.AddModelError("_FORM", T("The new password and confirmation password do not match."));
            }

            return ModelState.IsValid;
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

            List<LocalizedString> validationErrors;
            var validationResult = _membershipService.ValidateUser(userNameOrEmail, password, out validationErrors);
            if (validationResult == null) {
                _userEventHandler.LogInFailed(userNameOrEmail, password);
            }

            foreach (var error in validationErrors) {
                ModelState.AddModelError("_FORM", error);
            }

            return validationResult;
        }

        private bool ValidateRegistration(string userName, string email, string password, string confirmPassword) {
            bool validate = true;

            if (String.IsNullOrEmpty(userName)) {
                ModelState.AddModelError("username", T("You must specify a username."));
                validate = false;
            }
            else {
                if (userName.Length >= UserPart.MaxUserNameLength) {
                    ModelState.AddModelError("username", T("The username you provided is too long."));
                    validate = false;
                }
            }

            if (String.IsNullOrEmpty(email)) {
                ModelState.AddModelError("email", T("You must specify an email address."));
                validate = false;
            }
            else if (email.Length >= UserPart.MaxEmailLength) {
                ModelState.AddModelError("email", T("The email address you provided is too long."));
                validate = false;
            }
            else if (!Regex.IsMatch(email, UserPart.EmailPattern, RegexOptions.IgnoreCase)) {
                // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
                ModelState.AddModelError("email", T("You must specify a valid email address."));
                validate = false;
            }

            if (!validate)
                return false;

            if (!_userService.VerifyUserUnicity(userName, email)) {
                ModelState.AddModelError("userExists", T("User with that username and/or email already exists."));
            }

            ValidatePassword(password);

            if (!String.Equals(password, confirmPassword, StringComparison.Ordinal)) {
                ModelState.AddModelError("_FORM", T("The new password and confirmation password do not match."));
            }
            return ModelState.IsValid;
        }

        private void ValidatePassword(string password) {
            IDictionary<string, LocalizedString> validationErrors;

            if (!_userService.PasswordMeetsPolicies(password, out validationErrors)) {
                foreach (var error in validationErrors) {
                    ModelState.AddModelError(error.Key, error.Value);
                }
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus) {
            // See http://msdn.microsoft.com/en-us/library/system.web.security.membershipcreatestatus.aspx for
            // a full list of status codes.
            switch (createStatus) {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Username already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A username for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return
                        "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return
                        "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return
                        "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        #endregion
    }
}