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
            // Can the queries in this method be optimized away so that their results can be memorized
            // at least within the scope of a request?
            List<UserPermission> userRolesPermissions = new List<UserPermission>();
            IQueryable<UserPermission> userRolesPermissionsQuery = Enumerable.Empty<UserPermission>().AsQueryable();
            IQueryable<UserPermission> permissionsQuery = Enumerable.Empty<UserPermission>().AsQueryable();

            var currentUser = _authenticationService.GetAuthenticatedUser();
            if (currentUser != null) {
                // add the Authenticated role and its permissions
                // the Authenticated role is not assigned to the current user
                permissionsQuery = GetPermissionsFromRole("Authenticated");

                if (_authorizer.Authorize(StandardPermissions.SiteOwner)) {
                    // The SuperUser is a SiteOwner that has no assigned role. To properly manage
                    // that case we make up a "fake" UserPermission here to add to SiteOwners. We
                    // just need to make sure that the role we use there doesn't actually exist.
                    userRolesPermissions.Add(new UserPermission {
                        RoleName = SiteOwnerRoleName(),
                        PermissionName = "AllPermissions" // A SiteOWner has all Permissions
                    });
                    // A user with the SiteOwner permission may also have other roles
                    userRolesPermissionsQuery = _userRolesRepo
                      .Table.Where(usr => usr.UserId == currentUser.Id)
                      .Join(
                          _roleRepo.Table,
                          ur => ur.Role.Id,
                          r => r.Id,
                          (ur, r) => new UserPermission { RoleName = r.Name }
                      );
                    // Since SiteOwners have all permissions, we don't need to query for them here.
                    // We still query for their roles, because we may be displaying different stuff
                    // to users with different roles, even when they happen to have all permissions.
                }
                else {
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
            }
            else {
                // the anonymous user has no roles, get its permissions
                permissionsQuery = GetPermissionsFromRole("Anonymous");
            }

            if (userRolesPermissionsQuery.Any()) {
                userRolesPermissions.AddRange(userRolesPermissionsQuery
                    .ToList());
            }
            if (permissionsQuery.Any()) {
                userRolesPermissions.AddRange(permissionsQuery
                    .ToList());
            }

            if (userRolesPermissions.Any()) {

                var userRoles = String.Join(";", userRolesPermissions
                    .Select(r => r.RoleName)
                    .Distinct()
                    .OrderBy(s => s));

                var userPermissions = String.Join(";", userRolesPermissions
                    .Select(p => p.PermissionName)
                    .Distinct() // permissions may be duplicate: two different roles may give the same permission
                    .OrderBy(s => s));


                key.Append(string.Format("UserRoles={0};UserPermissions={1};",
                    userRoles.GetHashCode(),
                    userPermissions.GetHashCode()));
            }
            else {
                key.Append("UserRoles=;UserPermissions=;");
            }
        }

        private const string _siteOwnerRoleName = "SiteOwnerRole";
        private IEnumerable<string> _siteOwnerRoleNames;
        private string SiteOwnerRoleName() {
            if (_siteOwnerRoleNames == null) {
                // memorize this so it's only executed once per request
                _siteOwnerRoleNames = _roleRepo.Table
                    .Where(rr => rr.Name.StartsWith(_siteOwnerRoleName))
                    .Select(rr => rr.Name)
                    .ToList()
                    .Distinct() // sanity check
                    ;
            }

            var roleName = _siteOwnerRoleName;
            if (_siteOwnerRoleNames.Any() && _siteOwnerRoleNames.Contains(roleName)) {
                // compute unique and repeatable roleName
                var i = 0;
                do {
                    roleName = $"{_siteOwnerRoleName}-{i}";
                    i++;
                } while (_siteOwnerRoleNames.Contains(roleName));
            }
            return roleName;
        }

        private IQueryable<UserPermission> GetPermissionsFromRole(string role) {
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
    }
}