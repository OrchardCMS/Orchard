using System;
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
        private const int _version = 3;

        private readonly ShellSettings _settings;
        private readonly IClock _clock;
        // Lazy because Setup is using IAuthenticationService but doesn't have 
        // any implementation yet
        private readonly Lazy<IMembershipService> _membershipService; 
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISslSettingsProvider _sslSettingsProvider;
        private IUser _signedInUser;
        private bool _isAuthenticated;

        public FormsAuthenticationService(
            ShellSettings settings, 
            IClock clock, 
            Lazy<IMembershipService> membershipService, 
            IHttpContextAccessor httpContextAccessor,
            ISslSettingsProvider sslSettingsProvider) {
            _settings = settings;
            _clock = clock;
            _membershipService = membershipService;
            _httpContextAccessor = httpContextAccessor;
            _sslSettingsProvider = sslSettingsProvider;

            Logger = NullLogger.Instance;
            
            ExpirationTimeSpan = TimeSpan.FromDays(30);
        }

        public ILogger Logger { get; set; }

        public TimeSpan ExpirationTimeSpan { get; set; }

        public void SignIn(IUser user, bool createPersistentCookie) {
            var now = _clock.UtcNow;
            
            // the cookie user data is {userName.Base64};{tenant}
            // the username is encoded to base64 to prevent collisions with the ';' seprarator
            var userData = String.Concat(Convert.ToString(user.UserName).ToBase64(), ";", _settings.Name); 

            var ticket = new FormsAuthenticationTicket(
                _version,
                user.UserName,
                now,
                now.Add(ExpirationTimeSpan),
                createPersistentCookie,
                userData,
                FormsAuthentication.FormsCookiePath);

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket) {
                HttpOnly = true, // can't retrieve the cookie from JavaScript
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

            // if the cookie is from a previous format, ignore
            if (formsIdentity.Ticket.Version != _version) {
                return null;
            }

            var userData = formsIdentity.Ticket.UserData ?? "";

            // the cookie user data is {userName};{tenant}
            var userDataSegments = userData.Split(';');
            
            if (userDataSegments.Length < 2) {
                return null;
            }

            var userDataName = userDataSegments[0];
            var userDataTenant = userDataSegments[1];

            try {
                userDataName = userDataName.FromBase64();
            }
            catch {
                // if the cookie is tampered, it could contain a bad base64 value
                return null;
            }

            if (!String.Equals(userDataTenant, _settings.Name, StringComparison.OrdinalIgnoreCase)) {
                return null;
            }

            // todo: this issues a sql query for each authenticated request
            _signedInUser = _membershipService.Value.GetUser(userDataName);

            if(_signedInUser == null || !_membershipService.Value.CanAuthenticateWithCookie(_signedInUser)) {
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
