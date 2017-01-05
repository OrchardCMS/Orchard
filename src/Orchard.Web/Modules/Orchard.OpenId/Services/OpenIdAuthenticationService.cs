using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Security.Providers;
using Orchard.Services;

namespace Orchard.OpenId.Services {
    [OrchardFeature("Orchard.OpenId")]
    public class OpenIdAuthenticationService : IAuthenticationService {
        private readonly ShellSettings _settings;
        private readonly IClock _clock;
        private readonly IMembershipService _membershipService;
        private readonly ISslSettingsProvider _sslSettingsProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMembershipValidationService _membershipValidationService;
        private readonly IEnumerable<IOpenIdProvider> _openIdProviders;

        private IUser _localAuthenticationUser;

        IAuthenticationService _fallbackAuthenticationService;
        private IAuthenticationService FallbackAuthenticationService {
            get {
                if (_fallbackAuthenticationService == null)
                    _fallbackAuthenticationService = new FormsAuthenticationService(_settings, _clock, _membershipService, _httpContextAccessor, _sslSettingsProvider, _membershipValidationService);

                return _fallbackAuthenticationService;
            }
        }

        public OpenIdAuthenticationService(
            ShellSettings settings,
            IClock clock,
            IMembershipService membershipService,
            ISslSettingsProvider sslSettingsProvider,
            IHttpContextAccessor httpContextAccessor,
            IMembershipValidationService membershipValidationService,
            IEnumerable<IOpenIdProvider> openIdProviders) {

            _httpContextAccessor = httpContextAccessor;
            _membershipService = membershipService;
            _settings = settings;
            _clock = clock;
            _sslSettingsProvider = sslSettingsProvider;
            _membershipValidationService = membershipValidationService;
            _openIdProviders = openIdProviders;
        }

        public void SignIn(IUser user, bool createPersistentCookie) {
            if (IsFallbackNeeded()) {
                FallbackAuthenticationService.SignIn(user, createPersistentCookie);
            }
        }

        public void SignOut() {
            if (IsFallbackNeeded()) {
                FallbackAuthenticationService.SignOut();
            }
        }

        public void SetAuthenticatedUserForRequest(IUser user) {
            if (IsFallbackNeeded()) {
                FallbackAuthenticationService.SetAuthenticatedUserForRequest(user);
            }
        }

        public IUser GetAuthenticatedUser() {
            if (IsFallbackNeeded()) {
                return FallbackAuthenticationService.GetAuthenticatedUser();
            }

            var user = _httpContextAccessor.Current().GetOwinContext().Authentication.User;

            if (!user.Identity.IsAuthenticated) {
                return null;
            }

            // In memory caching of sorts since this method gets called many times per request
            if (_localAuthenticationUser != null) {
                return _localAuthenticationUser;
            }

            var userName = user.Identity.Name.Trim();

            //Get the local user, if local user account doesn't exist, create it 
            var localUser =
                _membershipService.GetUser(userName) ??
                _membershipService.CreateUser(new CreateUserParams(
                    userName, Membership.GeneratePassword(16, 1), userName, string.Empty, string.Empty, true
                ));

            return _localAuthenticationUser = localUser;
        }

        private bool IsLocalUser() {
            var anyClaim = _httpContextAccessor.Current().GetOwinContext().Authentication.User.Claims.FirstOrDefault();

            if (anyClaim == null || anyClaim.Issuer == Constants.General.LocalIssuer || anyClaim.Issuer == Constants.General.FormsIssuer) {
                return true;
            }

            return false;
        }

        private bool IsAnyProviderSettingsValid() {
            return _openIdProviders.Any(provider => provider.IsValid);
        }

        private bool IsFallbackNeeded() {
            return IsLocalUser() || !IsAnyProviderSettingsValid();
        }
    }
}