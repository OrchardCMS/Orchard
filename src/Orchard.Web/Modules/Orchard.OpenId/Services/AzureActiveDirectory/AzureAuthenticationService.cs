using System.Web;
using System.Web.Security;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Mvc;
using Orchard.OpenId.Models;
using Orchard.Security;
using Orchard.Security.Providers;
using Orchard.Services;
using Orchard.Settings;

namespace Orchard.OpenId.Services {
    [OrchardFeature("Orchard.OpenId.AzureActiveDirectory")]
    public class AzureAuthenticationService : IAuthenticationService {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMembershipService _membershipService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ShellSettings _settings;
        private readonly IClock _clock;
        private readonly ISslSettingsProvider _sslSettingsProvider;
        private readonly IMembershipValidationService _membershipValidationService;

        private IUser _localAuthenticationUser;

        IAuthenticationService _fallbackAuthenticationService;
        private IAuthenticationService FallbackAuthenticationService {
            get {
                if (_fallbackAuthenticationService == null)
                    _fallbackAuthenticationService = new FormsAuthenticationService(_settings, _clock, _membershipService, _httpContextAccessor, _sslSettingsProvider, _membershipValidationService);

                return _fallbackAuthenticationService;
            }
        }

        public AzureAuthenticationService(
            IHttpContextAccessor httpContextAccessor,
            IMembershipService membershipService,
            ShellSettings settings,
            IClock clock,
            ISslSettingsProvider sslSettingsProvider,
            IMembershipValidationService membershipValidationService,
            IWorkContextAccessor workContextAccessor) {

            _httpContextAccessor = httpContextAccessor;
            _membershipService = membershipService;
            _workContextAccessor = workContextAccessor;
            _settings = settings;
            _clock = clock;
            _sslSettingsProvider = sslSettingsProvider;
            _membershipValidationService = membershipValidationService;
        }

        public void SignIn(IUser user, bool createPersistentCookie) {
            if (!IsAzureSettingsValid()) {
                FallbackAuthenticationService.SignIn(user, createPersistentCookie);
            }
        }

        public void SignOut() {
            if (!IsAzureSettingsValid()) {
                FallbackAuthenticationService.SignOut();
            }
        }

        public void SetAuthenticatedUserForRequest(IUser user) {
            if (!IsAzureSettingsValid()) {
                FallbackAuthenticationService.SetAuthenticatedUserForRequest(user);
            }
        }

        public IUser GetAuthenticatedUser() {
            if (!IsAzureSettingsValid())
                return FallbackAuthenticationService.GetAuthenticatedUser();

            var azureUser = _httpContextAccessor.Current().GetOwinContext().Authentication.User;

            if (!azureUser.Identity.IsAuthenticated) {
                return null;
            }

            // In memory caching of sorts since this method gets called many times per request
            if (_localAuthenticationUser != null) {
                return _localAuthenticationUser;
            }

            var userName = azureUser.Identity.Name.Trim();

            //Get the local user, if local user account doesn't exist, create it 
            var localUser =
                _membershipService.GetUser(userName) ?? _membershipService.CreateUser(new CreateUserParams(
                    userName, Membership.GeneratePassword(16, 1), userName, string.Empty, string.Empty, true
                ));

            return _localAuthenticationUser = localUser;
        }

        private bool IsAzureSettingsValid() {
            try {
                WorkContext scope = _workContextAccessor.GetContext();
                AzureActiveDirectorySettingsPart azureSettings = scope.Resolve<ISiteService>().GetSiteSettings().As<AzureActiveDirectorySettingsPart>();

                return (azureSettings != null && azureSettings.IsValid);
            }
            catch {
                return false;
            }
        }
    }
}