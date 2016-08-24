using System.Web;
using System.Web.Security;
using Orchard.Mvc;
using Orchard.Security;

namespace Orchard.Azure.Authentication.Services {
    public class AzureAuthenticationService : IAuthenticationService {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMembershipService _membershipService;
        private IUser _localAuthenticationUser;

        public AzureAuthenticationService(IHttpContextAccessor httpContextAccessor, IMembershipService membershipService) {
            _httpContextAccessor = httpContextAccessor;
            _membershipService = membershipService;
        }

        public void SignIn(IUser user, bool createPersistentCookie) {}

        public void SignOut() {}

        public void SetAuthenticatedUserForRequest(IUser user) { }

        public IUser GetAuthenticatedUser() {
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
            var localUser = _membershipService.GetUser(userName) ?? _membershipService.CreateUser(new CreateUserParams(
                                userName, Membership.GeneratePassword(16, 1), userName, string.Empty, string.Empty, true    
                            ));

            return _localAuthenticationUser = localUser;
        }
    }
}