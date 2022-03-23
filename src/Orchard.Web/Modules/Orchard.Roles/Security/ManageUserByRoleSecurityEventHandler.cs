using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Roles.Constants;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Security;
using UserPermissions = Orchard.Users.Permissions;

namespace Orchard.Roles.Security {
    public class ManageUserByRoleSecurityEventHandler : IAuthorizationServiceEventHandler {

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IRoleService _roleService;

        private string _superUserName;
        public ManageUserByRoleSecurityEventHandler(
            IWorkContextAccessor workContextAccessor,
            IRoleService roleService) {

            _workContextAccessor = workContextAccessor;
            _roleService = roleService;

            _superUserName = _workContextAccessor.GetContext().CurrentSite.SuperUser;
        }
        public void Adjust(CheckAccessContext context) {
            if (!context.Granted
                && context.Permission == UserPermissions.ManageUsers) {
                // check that the user that is being managed is in the roles that
                // the current user is allowed to manage.
                var manager = context.User;
                var managed = context.Content.As<IUser>();
                if (manager != null) {

                    if (managed == null) {
                        // Not checking permission to manage a specific user
                        // Any "manage" permission is probably fine?
                        var rolesToCheck = _roleService.GetRoles()
                            .Select(r => r.Name).Except(SystemRoles.GetSystemRoles());
                        if (GrantPermission(rolesToCheck, manager, null)) {
                            context.Granted = true;
                            context.Adjusted = true;
                        }
                    } else {
                        // We allow the adjustments even if the user who's going to be managed is the
                        // SuperUser because:
                        // - retrocompatibility
                        // - If we don't, we may end up in a situation where nobody is able to manage
                        //   SuperUsers/SiteOwners

                        // Checking permission to manage a specific user
                        // The user we are attempting to manage must belong to to a subset of
                        // all those roles.
                        var theirRoleNames = managed
                            .GetRuntimeUserRoles()
                            // Never have to manage explicitly Anonymous or Authenticated roles
                            .Except(SystemRoles.GetSystemRoles());

                        if (GrantPermission(theirRoleNames, manager, managed)) {
                            context.Granted = true;
                            context.Adjusted = true;
                        }
                    }
                }
            }
        }

        private bool GrantPermission(
            IEnumerable<string> roleNamesToCheck,
            IUser manager, IUser managed) {

            IAuthorizationService authService;
            if (_workContextAccessor.GetContext().TryResolve<IAuthorizationService>(out authService)) {
                if (managed == null) {
                    // not checking on a specific user, so permission on any role is fine
                    return roleNamesToCheck.Any(rn =>
                        authService.TryCheckAccess(
                            ManageUserByRolePermissions.CreatePermissionForManageUsersInRole(rn),
                            manager, managed));
                } else {
                    // checking permissions on a specific user, so we need to have permissions
                    // to manage all their roles
                    return roleNamesToCheck.All(rn =>
                        authService.TryCheckAccess(
                            ManageUserByRolePermissions.CreatePermissionForManageUsersInRole(rn),
                            manager, managed));
                }
            }
            // if we can't test, fail the test
            return false;
        }

        public void Checking(CheckAccessContext context) { }

        public void Complete(CheckAccessContext context) { }
    }
}