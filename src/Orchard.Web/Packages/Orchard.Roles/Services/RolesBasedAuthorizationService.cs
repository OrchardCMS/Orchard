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
        private readonly IEnumerable<IAuthorizationServiceEvents> _events;

        public RolesBasedAuthorizationService(IRoleService roleService, IEnumerable<IAuthorizationServiceEvents> events) {
            _roleService = roleService;
            _events = events;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }

        #region Implementation of IAuthorizationService

        public void CheckAccess(Permission permission, IUser user, IContent content) {
            if (!TryCheckAccess(permission, user, content)) {
                throw new OrchardSecurityException { PermissionName = permission.Name };
            }
        }

        public bool TryCheckAccess(Permission permission, IUser user, IContent content) {
            var context = new CheckAccessContext { Permission = permission, User = user, Content = content };

            _events.Invoke(x => x.Checking(context), Logger);

            for (var adjustmentLimiter = 0; adjustmentLimiter != 3; ++adjustmentLimiter) {
                if (!context.Granted && context.User != null) {
                    if (String.Equals(context.User.UserName, "Administrator", StringComparison.OrdinalIgnoreCase) ||
                        ((!String.IsNullOrEmpty(CurrentSite.SuperUser) &&
                           String.Equals(context.User.UserName, CurrentSite.SuperUser, StringComparison.OrdinalIgnoreCase)))) {
                        context.Granted = true;
                    }
                }

                if (!context.Granted && context.User != null && context.User.Has<IUserRoles>()) {
                    var grantingNames = PermissionNames(context.Permission, Enumerable.Empty<string>()).ToArray();
                    IEnumerable<string> rolesForUser = context.User.As<IUserRoles>().Roles;

                    foreach (var role in rolesForUser) {
                        RoleRecord roleRecord = _roleService.GetRoleByName(role);
                        foreach (var permissionName in _roleService.GetPermissionsForRole(roleRecord.Id)) {
                            string possessedName = permissionName;
                            if (grantingNames.Any(grantingName => String.Equals(possessedName, grantingName, StringComparison.OrdinalIgnoreCase))) {
                                context.Granted = true;
                            }

                            if (context.Granted)
                                break;
                        }
                        
                        if (context.Granted)
                            break;
                    }
                }

                context.Adjusted = false;
                _events.Invoke(x => x.Adjust(context), Logger);
            }

            _events.Invoke(x => x.Complete(context), Logger);

            return context.Granted;
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
