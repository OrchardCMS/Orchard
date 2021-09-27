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
        private readonly IRepository<UserRolesPartRecord> _userRolesRepo;
        private readonly IRepository<RoleRecord> _roleRepo;
        private readonly IRepository<RolesPermissionsRecord> _rolesPermissionsRepo;
        private readonly IRepository<PermissionRecord> _permissionRepo;

        public CacheByRoleFilter(
            IAuthenticationService authenticationService,
            IRepository<UserRolesPartRecord> userRolesRepo,
            IRepository<RoleRecord> roleRepo,
            IRepository<RolesPermissionsRecord> rolesPermissionsRepo,
            IRepository<PermissionRecord> permissionRepo) {
            _authenticationService = authenticationService;
            _userRolesRepo = userRolesRepo;
            _roleRepo = roleRepo;
            _rolesPermissionsRepo = rolesPermissionsRepo;
            _permissionRepo = permissionRepo;
        }

        public void KeyGenerated(StringBuilder key) {
            var currentUser = _authenticationService.GetAuthenticatedUser();
            if (currentUser != null) {

                IQueryable<UserPermission> userRolesPermissionsQuery = _userRolesRepo
                    .Table.Where(usr => usr.UserId == currentUser.Id)
                    .Join(
                        _roleRepo.Table,
                        ur => ur.Role.Id,
                        r => r.Id,
                        (ur, r) => new { ContentItemRecord = r }
                    )
                    .Join(
                        _rolesPermissionsRepo.Table,
                        obj => obj.ContentItemRecord,
                        rp => rp.Role,
                        (obj, rp) => rp
                    )
                    .Join(
                        _permissionRepo.Table,
                        rp => rp.Permission.Id,
                        p => p.Id,
                        (rp, p) => new UserPermission { RoleName = rp.Role.Name, PermissionName = p.Name, FeatureName = p.FeatureName }
                    );

                IEnumerable<UserPermission> userRolesPermissions = userRolesPermissionsQuery.ToList();

                if (userRolesPermissions.Any()) {
                    var roles = userRolesPermissions
                        .Select(r => r.RoleName)
                        .Distinct()
                        .OrderBy(r => r)
                        .ToList();
                    
                    var permissions = userRolesPermissions
                        .OrderBy(p => p.FeatureName)
                        .ThenBy(p => p.PermissionName)
                        .Select(p => p.FeatureName+"."+ p.PermissionName)
                        .Distinct();

                    key.Append("UserRoles=" + String.Join(";", roles).GetHashCode() + ";UserPermissions="+ String.Join(";", permissions).GetHashCode() + ";");
                }
                else {
                    key.Append("UserRoles=;UserPermissions=;");
                }
            }
        }
    }
    public class UserPermission {
        public string RoleName { get; set; }
        public string PermissionName { get; set; }
        public string FeatureName { get; set; }
    }
}