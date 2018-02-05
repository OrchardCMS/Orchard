using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;
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

            ExpirationTimeSpan = TimeSpan.Zero;
        }

        public ILogger Logger { get; set; }

        public TimeSpan ExpirationTimeSpan {
            get; set;
            // The public setter allows injecting this from Sites.MyTenant.Config or Sites.config, by using
            // an AutoFac component 
        }

        public TimeSpan GetExpirationTimeSpan() {
            if (ExpirationTimeSpan != TimeSpan.Zero) {
                // Basically here we are checking whether a value has been injected. If that is the case
                // that takes priority over possible services. The idea is to make the existence of those
                // services not-breaking, so that the introduction of new ones will not affect tenants where
                // the value from the Sites.Config file has been used. Implementers of those services should
                // take care of noting this in whatever UI they provide for their configuration, for the sake
                // of clarity towards whoever handles the tenant's configuration.
                return ExpirationTimeSpan;
            }
            return _securityService.GetAuthenticationCookieLifeSpan();
        }

        public void SignIn(IUser user, bool createPersistentCookie) {

            CreateAndAddAuthCookie(user, createPersistentCookie);

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
            var userDataDictionary = new Dictionary<string, string>();
            
            if (formsIdentity.Ticket.Version == 3) {
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
                    return null;
                }
                userDataDictionary.Add("UserName", userDataName);
                userDataDictionary.Add("TenantName", userDataTenant);
            }
            else { //we assume that the version here will be 4
                try {
                    userDataDictionary = DeserializeUserData(userData);
                }
                catch (Exception) {
                    return null;
                }
            }

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

            // Upgrade old cookies
            if (formsIdentity.Ticket.Version < 4) {
                UpgradeAndAddAuthCookie(_signedInUser, formsIdentity.Ticket);
            }

            _isAuthenticated = true;
            return _signedInUser;
        }

        private HttpCookie UpgradeAndAddAuthCookie(IUser user, FormsAuthenticationTicket oldTicket) {
            var ticket = UpgradeAuthenticationTicket(user, oldTicket);

            var cookie = CreateCookieFromTicket(ticket);

            var httpContext = _httpContextAccessor.Current();
            httpContext.Response.Cookies.Add(cookie);

            return cookie;
        }

        private HttpCookie CreateAndAddAuthCookie(IUser user, bool createPersistentCookie) {
            var ticket = NewAuthenticationTicket(user, createPersistentCookie);

            var cookie = CreateCookieFromTicket(ticket);

            var httpContext = _httpContextAccessor.Current();
            httpContext.Response.Cookies.Add(cookie);

            return cookie;
        }

        private HttpCookie CreateCookieFromTicket(FormsAuthenticationTicket ticket) {
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

            if (ticket.IsPersistent) {
                cookie.Expires = ticket.Expiration;
            }

            return cookie;
        }

        private FormsAuthenticationTicket NewAuthenticationTicket(IUser user, bool createPersistentCookie) {
            var now = _clock.UtcNow.ToLocalTime();

            var userData = ComputeUserData(user);

            return new FormsAuthenticationTicket(
                _cookieVersion,
                user.UserName,
                now,
                now.Add(GetExpirationTimeSpan()),
                createPersistentCookie,
                userData,
                FormsAuthentication.FormsCookiePath);
        }

        private FormsAuthenticationTicket UpgradeAuthenticationTicket(IUser user, FormsAuthenticationTicket oldTicket) {
            var userData = ComputeUserData(user);

            return new FormsAuthenticationTicket(
                _cookieVersion,
                user.UserName,
                oldTicket.IssueDate,
                oldTicket.Expiration,
                oldTicket.IsPersistent,
                userData,
                FormsAuthentication.FormsCookiePath);
        }

        private Dictionary<string, string> ComputeUserDataDictionary(IUser user) {
            var userDataDictionary = new Dictionary<string, string>();
            userDataDictionary.Add("UserName", user.UserName);
            foreach (var userDataProvider in _userDataProviders) {
                var key = userDataProvider.Key;
                var value = userDataProvider.ComputeUserDataElement(user);
                if (key != null && value != null) {
                    userDataDictionary.Add(key, value);
                }
            }
            return userDataDictionary;
        }

        private string ComputeUserData(IUser user) {
            // serialize dictionary to userData string
            return SerializeUserDataDictionary(ComputeUserDataDictionary(user));
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
        // Use Newtonsoft.Json to handle this
        private string SerializeUserDataDictionary(IDictionary<string, string> userDataDictionary) {
            return JsonConvert.SerializeObject(userDataDictionary, Formatting.None);
        }

        private Dictionary<string, string> DeserializeUserData(string userData) {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(userData);
        }

        #endregion
    }
}
