using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Services;
using Orchard.Utility.Extensions;

namespace Orchard.Security.Providers {
    public class FormsAuthenticationService : IAuthenticationService {
        private const int _cookieVersion = 4;

        private readonly ShellSettings _settings;
        private readonly IClock _clock;
        private readonly IMembershipService _membershipService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISslSettingsProvider _sslSettingsProvider;
        private readonly IMembershipValidationService _membershipValidationService;
        private readonly IEnumerable<IUserDataValidationProvider> _userDataValidationProviders;

        private IUser _signedInUser;
        private bool _isAuthenticated;

        public FormsAuthenticationService(
            ShellSettings settings, 
            IClock clock, 
            IMembershipService membershipService,
            IHttpContextAccessor httpContextAccessor,
            ISslSettingsProvider sslSettingsProvider,
            IMembershipValidationService membershipValidationService,
            IEnumerable<IUserDataValidationProvider> userDataValidationProviders) {
            _settings = settings;
            _clock = clock;
            _membershipService = membershipService;
            _httpContextAccessor = httpContextAccessor;
            _sslSettingsProvider = sslSettingsProvider;
            _membershipValidationService = membershipValidationService;
            _userDataValidationProviders = userDataValidationProviders;

            Logger = NullLogger.Instance;
            
            ExpirationTimeSpan = TimeSpan.FromDays(30);
        }

        public ILogger Logger { get; set; }

        public TimeSpan ExpirationTimeSpan { get; set; }

        public void SignIn(IUser user, bool createPersistentCookie) {
            var now = _clock.UtcNow.ToLocalTime();

            // The cookie user data is "{userName.Base64};{providerKey|providerValue};{providerKey|providerValue}....".
            // The UserName is encoded to Base64 to prevent collisions with the ';' and '|' seprarators.
            var userData = user.UserName.ToBase64();

            foreach (var userDataValidationProvider in _userDataValidationProviders) {
                try {
                    // The data provided by this provider is base 64 encoded to avoid collisions with the ';' seprarator.
                    userData += String.Format(";{0}|{1}", userDataValidationProvider.Key.ToBase64(), userDataValidationProvider.GetUserData().ToBase64());
                }
                catch (Exception ex) {
                    Logger.Error(ex, "An exception was thrown by {0} when generating the values to be added to User Data.", userDataValidationProvider.GetType().FullName);

                    throw;
                }
            }

            var ticket = new FormsAuthenticationTicket(
                _cookieVersion,
                user.UserName,
                now,
                now.Add(ExpirationTimeSpan),
                createPersistentCookie,
                userData,
                FormsAuthentication.FormsCookiePath);

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket) {
                HttpOnly = true,
                Secure = _sslSettingsProvider.GetRequiresSSL(),
                Path = FormsAuthentication.FormsCookiePath
            };

            var httpContext = _httpContextAccessor.Current();

            if (!String.IsNullOrEmpty(_settings.RequestUrlPrefix)) {
                cookie.Path = GetCookiePath(httpContext);
            }

            if (FormsAuthentication.CookieDomain != null) {
                cookie.Domain = FormsAuthentication.CookieDomain;
            }

            if (createPersistentCookie) {
                cookie.Expires = ticket.Expiration;
            }
            
            httpContext.Response.Cookies.Add(cookie);

            _isAuthenticated = true;
            _signedInUser = user;
        }

        public void SignOut() {
            _signedInUser = null;
            _isAuthenticated = false;
            FormsAuthentication.SignOut();

            // overwritting the authentication cookie for the given tenant
            var httpContext = _httpContextAccessor.Current();
            var rFormsCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "") {
                Expires = DateTime.Now.AddYears(-1),
            };

            if (!String.IsNullOrEmpty(_settings.RequestUrlPrefix)) {
                rFormsCookie.Path = GetCookiePath(httpContext);
            }

            httpContext.Response.Cookies.Add(rFormsCookie);
        }

        public void SetAuthenticatedUserForRequest(IUser user) {
            _signedInUser = user;
            _isAuthenticated = true;
        }

        public IUser GetAuthenticatedUser() {
            if (_signedInUser != null || _isAuthenticated)
                return _signedInUser;

            var httpContext = _httpContextAccessor.Current();
            if (httpContext.IsBackgroundContext() || !httpContext.Request.IsAuthenticated || !(httpContext.User.Identity is FormsIdentity)) {
                return null;
            }

            var formsIdentity = (FormsIdentity)httpContext.User.Identity;
            var userData = formsIdentity.Ticket.UserData ?? "";

            // The cookie user data is "{userName.Base64};{providerKey|providerValue};{providerKey|providerValue}....".
            var userDataSegments = userData.Split(';');

            var userDataName = userDataSegments[0];

            try {
                userDataName = userDataName.FromBase64();
            }
            catch {
                return null;
            }

            // Iterate over all but the first user data segment because the first is the username
            for (var i = 1; i < userDataSegments.Length; i++) {
                var segmentSplit = userDataSegments[i].Split('|');

                if (segmentSplit.Length < 2) {
                    continue;
                }

                var providerKey = segmentSplit[0].FromBase64();
                var providerValue = segmentSplit[1].FromBase64();

                foreach (var userDataValidationProvider in _userDataValidationProviders.Where(p=>p.Key == providerKey)) {
                    try {
                        if (!userDataValidationProvider.ValidateUserData(providerValue)) {
                            return null;
                        }   
                    }
                    catch (Exception ex) {
                        Logger.Error(ex, "An exception was thrown by {0} when validating the values found in User Data.", userDataValidationProvider.GetType().FullName);

                        // Throwing here would result in a YSOD. Instead, just return null, indicating that the user is not logged in.
                        return null;
                    }
                }
            }

            _signedInUser = _membershipService.GetUser(userDataName);
            if (_signedInUser == null || !_membershipValidationService.CanAuthenticateWithCookie(_signedInUser)) {
                return null;
            }

            _isAuthenticated = true;
            return _signedInUser;
        }

        private string GetCookiePath(HttpContextBase httpContext) {
            var cookiePath = httpContext.Request.ApplicationPath;
            if (cookiePath != null && cookiePath.Length > 1) {
                cookiePath += '/';
            }

            cookiePath += _settings.RequestUrlPrefix;

            return cookiePath;
        }
    }
}
