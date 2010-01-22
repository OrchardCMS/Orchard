using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Logging;
using Orchard.ContentManagement;
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
        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }

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

            var grantingNames = PermissionNames(permission, Enumerable.Empty<string>()).ToArray();

            IEnumerable<string> rolesForUser = user.As<IUserRoles>().Roles;
            foreach (var role in rolesForUser) {
                RoleRecord roleRecord = _roleService.GetRoleByName(role);
                foreach (var permissionName in _roleService.GetPermissionsForRole(roleRecord.Id)) {
                    string possessedName = permissionName;
                    if (grantingNames.Any(grantingName => String.Equals(possessedName, grantingName, StringComparison.OrdinalIgnoreCase))) {
                        return true;
                    }
                }
            }

            return false;
        }

        private static IEnumerable<string> PermissionNames(Permission permission, IEnumerable<string> stack) {
            // the given name is tested
            yield return permission.Name;

            // iterate implied permissions to grant, it present
            if (permission.ImpliedBy != null && permission.ImpliedBy.Any()) {
                foreach (var impliedBy in permission.ImpliedBy) {
                    // avoid potential recursion
                    if (stack.Contains(impliedBy.Name))
                        continue;

                    // otherwise accumulate the implied permission names recursively
                    foreach (var impliedName in PermissionNames(impliedBy, stack.Concat(new[] { permission.Name }))) {
                        yield return impliedName;
                    }
                }
            }
        }

        #endregion
    }
}
