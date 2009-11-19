using System;
using System.Collections.Generic;
using Orchard.Logging;
using Orchard.Models;
using Orchard.Roles.Models.NoRecord;
using Orchard.Roles.Records;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Settings;

namespace Orchard.Roles.Services {
    public class RolesBasedAuthorizationService : IAuthorizationService {
        private readonly IRoleService _roleService;

        public RolesBasedAuthorizationService(IRoleService roleService) {
            _roleService = roleService;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public ISite CurrentSite { get; set; }

        #region Implementation of IAuthorizationService

        public bool CheckAccess(IUser user, Permission permission) {
            if (user == null) {
                return false;
            }

            if (String.Equals(user.UserName, "Administrator", StringComparison.OrdinalIgnoreCase) || 
                ((!String.IsNullOrEmpty(CurrentSite.SuperUser) &&
                   String.Equals(user.UserName, CurrentSite.SuperUser, StringComparison.OrdinalIgnoreCase)))) {
                return true;
            }

            IEnumerable<string> rolesForUser = user.As<IUserRoles>().Roles;
            foreach (var role in rolesForUser) {
                RoleRecord roleRecord = _roleService.GetRoleByName(role);
                foreach (var permissionName in _roleService.GetPermissionsForRole(roleRecord.Id)) {
                    if (String.Equals(permissionName, permission.Name, StringComparison.OrdinalIgnoreCase)) {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion
    }
}
