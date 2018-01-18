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
        private const int _cookieVersion = 3;

        private readonly ShellSettings _settings;
        private readonly IClock _clock;
        private readonly IMembershipService _membershipService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISslSettingsProvider _sslSettingsProvider;
        private readonly IMembershipValidationService _membershipValidationService;
        private readonly IEnumerable<IUserDataProvider> _userDataProviders;
        private readonly ISecurityService _securityService;

        private IUser _signedInUser;
        private bool _isAuthenticated;

        // This fixes a performance issue when the forms authentication cookie is set to a
        // user name not mapped to an actual Orchard user content item. If the request is
        // authenticated but a null user is returned, multiple calls to GetAuthenticatedUser
        // will cause multiple DB invocations, slowing down the request. We therefore
        // remember if the current user is a non-Orchard user between invocations.
        private bool _isNonOrchardUser;

        public FormsAuthenticationService(
            ShellSettings settings,
            IClock clock,
            IMembershipService membershipService,
            IHttpContextAccessor httpContextAccessor,
            ISslSettingsProvider sslSettingsProvider,
            IMembershipValidationService membershipValidationService,
            IEnumerable<IUserDataProvider> userDataProviders,
            ISecurityService securityService) {

            _settings = settings;
            _clock = clock;
            _membershipService = membershipService;
            _httpContextAccessor = httpContextAccessor;
            _sslSettingsProvider = sslSettingsProvider;
            _membershipValidationService = membershipValidationService;
            _userDataProviders = userDataProviders;
            _securityService = securityService;

            Logger = NullLogger.Instance;

            ExpirationTimeSpan = _securityService.GetAuthenticationCookieLifeSpan();
        }

        public ILogger Logger { get; set; }

        public TimeSpan ExpirationTimeSpan { get; set; }

        public void SignIn(IUser user, bool createPersistentCookie) {
            var now = _clock.UtcNow.ToLocalTime();

            var userDataDictionary = new Dictionary<string, string>();
            userDataDictionary.Add("UserName", user.UserName);
            foreach (var userDataProvider in _userDataProviders) {
                userDataDictionary.Add(
                    userDataProvider.Key,
                    userDataProvider.ComputeUserDataElement(user));
            }
            // serialize dictionary to userData string
            var userData = SerializeUserDataDictionary(userDataDictionary);

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
            _isNonOrchardUser = false;
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
            _isNonOrchardUser = false;
        }

        public IUser GetAuthenticatedUser() {

            if (_isNonOrchardUser)
                return null;

            if (_signedInUser != null || _isAuthenticated)
                return _signedInUser;

            var httpContext = _httpContextAccessor.Current();
            if (httpContext.IsBackgroundContext() || !httpContext.Request.IsAuthenticated || !(httpContext.User.Identity is FormsIdentity)) {
                return null;
            }

            var formsIdentity = (FormsIdentity)httpContext.User.Identity;
            var userData = formsIdentity.Ticket.UserData ?? "";

            // deserialize userData string to Dictionary<string, string> for provders
            var userDataDictionary = DeserializeUserData(userData);
            // 1. Take the username
            if (!userDataDictionary.ContainsKey("UserName")) {
                return null; // should never happen, unless the cookie has been tampered with
            }
            var userName = userDataDictionary["UserName"];
            _signedInUser = _membershipService.GetUser(userName);
            if (_signedInUser == null || !_membershipValidationService.CanAuthenticateWithCookie(_signedInUser)) {
                _isNonOrchardUser = true;
                return null;
            }
            // 2. Check the other stuff from the dictionary
            var validLogin = _userDataProviders.All(udp => udp.IsValid(_signedInUser, userDataDictionary));
            if (!validLogin) {
                _signedInUser = null;
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

        #region Serialization of UserData Dictionary
        // both keys and values are converted to base64 strings.
        // the key and value of a pair are separated by a pipe character "|"
        // pairs are separated by a semicolon ";"
        // These custome methopds are to avoid a dependency 
        private string SerializeUserDataDictionary(IDictionary<string, string> userDataDictionary) {
            
            return string.Join(";", userDataDictionary.Select(kvp =>
                string.Join("|", kvp.Key.ToBase64(), kvp.Value.ToBase64())));
        }

        private Dictionary<string, string> DeserializeUserData(string userData) {
            var dictionary = new Dictionary<string, string>();

            var serializedPairs = userData.Split(';');
            foreach (var sKvp in serializedPairs) {
                var elements = sKvp.Split('|');
                if (elements.Length != 2) {
                    continue;
                }
                if (dictionary.ContainsKey(elements[0])) {
                    continue; // keys should be unique
                }
                dictionary.Add(elements[0].FromBase64(), elements[1].FromBase64());
            }

            return dictionary;
        }

        #endregion
    }
}
