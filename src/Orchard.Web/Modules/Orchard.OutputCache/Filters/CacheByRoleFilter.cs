using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Filters;
using Orchard.Roles.Models;
using Orchard.Security;

namespace Orchard.OutputCache.Filters {
    [OrchardFeature("Orchard.OutputCache.CacheByRole")]
    public class CacheByRoleFilter : ICachingEventHandler {
        private readonly IAuthenticationService _authenticationService;
        public CacheByRoleFilter(IAuthenticationService authenticationService) {
            _authenticationService = authenticationService;
        }

        public void KeyGenerated(StringBuilder key) {
            var currentUser = _authenticationService.GetAuthenticatedUser();
            if (currentUser != null) {
                var roles = currentUser.As<UserRolesPart>().Roles;
                if (roles.Any()) {
                    // append roles in alphabetical order
                    key.Append("UserRoles=" + 
                        String.Join("|",roles.OrderBy(r => r))+";");
                }
                else {
                    key.Append("UserRoles=empty-role;");
                }
            }
        }
    }
}