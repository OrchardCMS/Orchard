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

        private Lazy<IAuthorizationService> _authorizationService;

        private string _superUserName;
        public ManageUserByRoleSecurityEventHandler(
            IWorkContextAccessor workContextAccessor,
            IRoleService roleService) {

            _workContextAccessor = workContextAccessor;
            _roleService = roleService;

            _superUserName = _workContextAccessor.GetContext().CurrentSite.SuperUser;
            _authorizationService = new Lazy<IAuthorizationService>(() =>
                _workContextAccessor.GetContext().Resolve<IAuthorizationService>());
            _allRoleNames = _roleService
                .GetRoles()
                .Select(r => r.Name)
                .Except(SystemRoles.GetSystemRoles());
        }

        // memorize this to avoid fetching this information potentially several times per request
        private IEnumerable<string> _allRoleNames;

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
                        var rolesToCheck = _allRoleNames;
                        if (GrantPermission(rolesToCheck, manager, null)) {
                            context.Granted = true;
                            context.Adjusted = true;
                        }
                    } else {
                        // We prevent Manage permissions on specific roles to affect the SuperUser. Only users
                        // that actually have the full ManageUsers permissions will be able to manage them.
                        if(!IsSuperUser(managed)) {
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
        }

        private bool GrantPermission(
            IEnumerable<string> roleNamesToCheck,
            IUser manager, IUser managed) {

            if (_authorizationService.Value != null) {
                if (managed == null) {
                    // not checking on a specific user, so permission on any role is fine
                    return roleNamesToCheck.Any(rn =>
                        _authorizationService.Value.TryCheckAccess(
                            ManageUserByRolePermissions.CreatePermissionForManageUsersInRole(rn),
                            manager, managed));
                } else {
                    // checking permissions on a specific user, so we need to have permissions
                    // to manage all their roles
                    if (roleNamesToCheck.Any()) {
                        return roleNamesToCheck.All(rn =>
                            _authorizationService.Value.TryCheckAccess(
                                ManageUserByRolePermissions.CreatePermissionForManageUsersInRole(rn),
                                manager, managed));
                    } else {
                        // if the specific user has no assigned role, they are just an "Authenticated" user.
                        // Enumerable.All applied to that would return true, which may not be correct. We
                        // only wish to return true if the user has any of the ManageUserByRole Permission.
                        // To verify that, we test across all possible roles.
                        return _allRoleNames
                            .Any(rn =>
                               _authorizationService.Value.TryCheckAccess(
                                   ManageUserByRolePermissions.CreatePermissionForManageUsersInRole(rn),
                                   manager, managed));
                    }
                }
            }
            // if we can't test, fail the test
            return false;
        }

        private bool IsSuperUser(IUser user) {

            var isSuperUser = string.Equals(user.UserName, _superUserName);
            // We could be testing the SiteOwner permission as well but:
            // - user can only have that permission if they belong to a Role the Permission is assigned to.
            // - if we can manage that role, then there's no reason why we should prevent it from being
            //   managed here.
            return isSuperUser;
        }

        public void Checking(CheckAccessContext context) { }

        public void Complete(CheckAccessContext context) { }
    }
}