using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Roles.Models;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Settings;

namespace Orchard.Roles.Services {
    [UsedImplicitly]
    public class RolesBasedAuthorizationService : IAuthorizationService {
        private readonly IRoleService _roleService;
        private readonly IAuthorizationServiceEventHandler _authorizationServiceEventHandler;
        private static readonly string[] AnonymousRole = new[] { "Anonymous" };
        private static readonly string[] AuthenticatedRole = new[] { "Authenticated" };

        public RolesBasedAuthorizationService(IRoleService roleService, IAuthorizationServiceEventHandler authorizationServiceEventHandler) {
            _roleService = roleService;
            _authorizationServiceEventHandler = authorizationServiceEventHandler;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }


        public void CheckAccess(Permission permission, IUser user, IContent content) {
            if (!TryCheckAccess(permission, user, content)) {
                throw new OrchardSecurityException { PermissionName = permission.Name };
            }
        }

        public bool TryCheckAccess(Permission permission, IUser user, IContent content) {
            var context = new CheckAccessContext { Permission = permission, User = user, Content = content };
            _authorizationServiceEventHandler.Checking(context);

            for (var adjustmentLimiter = 0; adjustmentLimiter != 3; ++adjustmentLimiter) {
                if (!context.Granted && context.User != null) {
                    if (String.Equals(context.User.UserName, "Administrator", StringComparison.OrdinalIgnoreCase) ||
                        ((!String.IsNullOrEmpty(CurrentSite.SuperUser) &&
                           String.Equals(context.User.UserName, CurrentSite.SuperUser, StringComparison.OrdinalIgnoreCase)))) {
                        context.Granted = true;
                    }
                }

                if (!context.Granted) {

                    // determine which set of permissions would satisfy the access check
                    var grantingNames = PermissionNames(context.Permission, Enumerable.Empty<string>()).ToArray();

                    // determine what set of roles should be examined by the access check
                    IEnumerable<string> rolesToExamine;
                    if (context.User == null) {
                        rolesToExamine = AnonymousRole;
                    }
                    else if (context.User.Has<IUserRoles>()) {
                        rolesToExamine = context.User.As<IUserRoles>().Roles.Concat(AuthenticatedRole);
                    }
                    else {
                        rolesToExamine = AuthenticatedRole;
                    }

                    foreach (var role in rolesToExamine) {
                        RoleRecord roleRecord = _roleService.GetRoleByName(role);
                        if ( roleRecord == null )
                            continue;
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
                _authorizationServiceEventHandler.Adjust(context);
                if (!context.Adjusted)
                    break;
            }

            _authorizationServiceEventHandler.Complete(context);

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

    }
}
