using Orchard.Logging;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Roles.Services {
    public class RolesBasedAuthorizationService : IAuthorizationService {
        private readonly IRoleService _roleService;

        public RolesBasedAuthorizationService(IRoleService roleService) {
            _roleService = roleService;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        #region Implementation of IAuthorizationService

        public bool CheckAccess(IUser user, Permission permission) {
            //TODO: Get roles for user
            //TODO: Get permissions for Roles of the IUser from the role service
            //TODO: Return false if current user doesn't have the permission
            return true;
        }

        #endregion
    }
}
