using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Roles.Models;
using Orchard.Security;

namespace Orchard.OutputCache.Filters {
    [OrchardFeature("Orchard.OutputCache.CacheByRole")]
    public class CacheByRoleFilter : ICachingEventHandler {
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizer _authorizer;
        private readonly IRepository<UserRolesPartRecord> _userRolesRepo;
        private readonly IRepository<RoleRecord> _roleRepo;
        private readonly IRepository<RolesPermissionsRecord> _rolesPermissionsRepo;
        private readonly IRepository<PermissionRecord> _permissionRepo;

        public CacheByRoleFilter(
            IAuthenticationService authenticationService,
            IAuthorizer authorizer,
            IRepository<UserRolesPartRecord> userRolesRepo,
            IRepository<RoleRecord> roleRepo,
            IRepository<RolesPermissionsRecord> rolesPermissionsRepo,
            IRepository<PermissionRecord> permissionRepo) {
            _authenticationService = authenticationService;
            _authorizer = authorizer;
            _userRolesRepo = userRolesRepo;
            _roleRepo = roleRepo;
            _rolesPermissionsRepo = rolesPermissionsRepo;
            _permissionRepo = permissionRepo;
        }

        public void KeyGenerated(StringBuilder key) {
            HashSet<UserPermission> userRolesPermissions = new HashSet<UserPermission>();
            IQueryable<UserPermission> userRolesPermissionsQuery = Enumerable.Empty<UserPermission>().AsQueryable();
            IQueryable<UserPermission> permissionsQuery = Enumerable.Empty<UserPermission>().AsQueryable();

            var currentUser = _authenticationService.GetAuthenticatedUser();
            if (currentUser != null) {
                // add the Authenticated role and its permissions
                // the Authenticated role is not assigned to the current user
                permissionsQuery = GetPermissioFromRole("Authenticated");

                if (_authorizer.Authorize(StandardPermissions.SiteOwner)) {
                    // the site owner has no permissions
                    // get the roles of the site owner
                    userRolesPermissionsQuery = _userRolesRepo
                      .Table.Where(usr => usr.UserId == currentUser.Id)
                      .Join(
                          _roleRepo.Table,
                          ur => ur.Role.Id,
                          r => r.Id,
                          (ur, r) => r
                      )
                      .Select(urp=>new UserPermission { RoleName = urp.Name });
                } else {
                    userRolesPermissionsQuery = _userRolesRepo
                        // get user roles and permissions
                        .Table.Where(usr => usr.UserId == currentUser.Id)
                        // given the ids of the roles related to the user
                        // join with the RoleRecord table
                        // get the role name
                        .Join(
                            _roleRepo.Table,
                            ur => ur.Role.Id,
                            r => r.Id,
                            (ur, r) => new { ContentItemRecord = r }
                        )
                        // join table RolePermissionRecord
                        // for each role, get id of role permissions
                        .Join(
                            _rolesPermissionsRepo.Table,
                            obj => obj.ContentItemRecord,
                            rp => rp.Role,
                            (obj, rp) => rp
                        )
                        // join PermissionRecord
                        // for each id permission get feature and permission name
                        .Join(
                            _permissionRepo.Table,
                            rp => rp.Permission.Id,
                            p => p.Id,
                            (rp, p) => new UserPermission { RoleName = rp.Role.Name, PermissionName = p.FeatureName + "." + p.Name }
                        );
                }
            } else {
                // the anonymous user has no roles, get its permissions
                permissionsQuery = GetPermissioFromRole("Anonymous");
            }

            if (userRolesPermissionsQuery.Any()) {
                userRolesPermissions.UnionWith(userRolesPermissionsQuery
                    .OrderBy(urp => urp.RoleName)
                    .ThenBy(urp => urp.PermissionName)
                    .ToHashSet());
            }
            if (permissionsQuery.Any()) {
                userRolesPermissions.UnionWith(permissionsQuery
                    .OrderBy(urp => urp.RoleName)
                    .ThenBy(urp => urp.PermissionName)
                    .ToHashSet());
            }

            if (userRolesPermissions.Any()) {
                
                var userRoles = String.Join(";", userRolesPermissions
                    .Select(r => r.RoleName)
                    // .Distinct() // roles should already be unique
                    .OrderBy(s => s));

                var userPermissions = String.Join(";", userRolesPermissions
                    .Select(p => p.PermissionName)
                    .Distinct() // permissions may be duplicate: two different roles may give the same permission
                    .OrderBy(s => s));


                key.Append(string.Format("UserRoles={0};UserPermissions={1};",
                    userRoles.GetHashCode(),
                    userPermissions.GetHashCode()));
            } else {
                key.Append("UserRoles=;UserPermissions=;");
            }
        }

        private IQueryable<UserPermission> GetPermissioFromRole(string role) {
            return _roleRepo
                .Table.Where(r => r.Name == role)
                .Join(
                    _rolesPermissionsRepo.Table,
                    r => r.Id,
                    rp => rp.Role.Id,
                    (obj, rp) => rp
                )
                .Join(
                    _permissionRepo.Table,
                    rp => rp.Permission.Id,
                    p => p.Id,
                    (rp, p) => new UserPermission { RoleName = rp.Role.Name, PermissionName = p.FeatureName + "." + p.Name }
                );
        }
    }
    public class UserPermission {
        public UserPermission() {
            RoleName = string.Empty;
            PermissionName = string.Empty;
        }
        public string RoleName { get; set; }
        public string PermissionName { get; set; }

        public override int GetHashCode() {
            return $"{RoleName}.{PermissionName}".GetHashCode();
        }
        public override bool Equals(object obj) {
            var other = (UserPermission)obj;
            if (other != null) {
                return ((RoleName == null && other.RoleName == null)
                        || (RoleName != null && RoleName.Equals(other.RoleName)))
                    && ((PermissionName == null && other.PermissionName == null)
                        || (PermissionName != null && PermissionName.Equals(other.PermissionName)));
            }
            return false;
        }
    }
}