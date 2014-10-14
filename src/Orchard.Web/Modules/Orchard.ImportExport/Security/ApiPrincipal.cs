using System.Security.Principal;
using Orchard.ContentManagement;
using Orchard.Roles.Models;
using Orchard.Security;

namespace Orchard.ImportExport.Security {
    public class ApiPrincipal : IPrincipal {
        private readonly IUser _user;
        public ApiPrincipal(IUser user) {
            _user = user;
        }

        public bool IsInRole(string role) {
            return _user.As<UserRolesPart>().Roles.Contains(role);
        }

        public IIdentity Identity { get { return new ApiIdentity(_user); } }
    }
}