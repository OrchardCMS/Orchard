using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Roles.Models;
using Orchard.Security.Permissions;

namespace Orchard.Roles.Services {
    [UsedImplicitly]
    public class RoleService : IRoleService {
        private readonly IRepository<RoleRecord> _roleRepository;
        private readonly IRepository<PermissionRecord> _permissionRepository;
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;

        public RoleService(IRepository<RoleRecord> roleRepository,
                           IRepository<PermissionRecord> permissionRepository,
                           IEnumerable<IPermissionProvider> permissionProviders) {
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _permissionProviders = permissionProviders;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public IEnumerable<RoleRecord> GetRoles() {
            var roles = from role in _roleRepository.Table select role;
            return roles.ToList();
        }

        public RoleRecord GetRole(int id) {
            return _roleRepository.Get(id);
        }

        public RoleRecord GetRoleByName(string name) {
            return _roleRepository.Get(x => x.Name == name);
        }

        public void CreateRole(string roleName) {
            _roleRepository.Create(new RoleRecord { Name = roleName });
        }

        public void CreatePermissionForRole(string roleName, string permissionName) {
            if (_permissionRepository.Get(x => x.Name == permissionName) == null) {
                _permissionRepository.Create(new PermissionRecord {
                    Description = GetPermissionDescription(permissionName),
                    Name = permissionName,
                    FeatureName = GetFeatureName(permissionName)
                });
            }
            RoleRecord roleRecord = GetRoleByName(roleName);
            PermissionRecord permissionRecord = _permissionRepository.Get(x => x.Name == permissionName);
            roleRecord.RolesPermissions.Add(new RolesPermissionsRecord { Permission = permissionRecord, Role = roleRecord });
        }

        public void UpdateRole(int id, string roleName, IEnumerable<string> rolePermissions) {
            RoleRecord roleRecord = GetRole(id);
            roleRecord.Name = roleName;
            roleRecord.RolesPermissions.Clear();
            foreach (var rolePermission in rolePermissions) {
                string permission = rolePermission;
                if (_permissionRepository.Get(x => x.Name == permission) == null) {
                    _permissionRepository.Create(new PermissionRecord {
                        Description = GetPermissionDescription(permission),
                        Name = permission,
                        FeatureName = GetFeatureName(permission)
                    });
                }
                PermissionRecord permissionRecord = _permissionRepository.Get(x => x.Name == permission);
                roleRecord.RolesPermissions.Add(new RolesPermissionsRecord { Permission = permissionRecord, Role = roleRecord });
            }
        }

        private string GetFeatureName(string permissionName) {
            foreach (var permissionProvider in _permissionProviders) {
                foreach (var permission in permissionProvider.GetPermissions()) {
                    if (String.Equals(permissionName, permission.Name, StringComparison.OrdinalIgnoreCase)) {
                        return permissionProvider.Feature.Descriptor.Id;
                    }
                }
            }
            throw new ArgumentException(T("Permission {0} was not found in any of the installed modules.", permissionName).ToString());
        }

        private string GetPermissionDescription(string permissionName) {
            foreach (var permissionProvider in _permissionProviders) {
                foreach (var permission in permissionProvider.GetPermissions()) {
                    if (String.Equals(permissionName, permission.Name, StringComparison.OrdinalIgnoreCase)) {
                        return permission.Description;
                    }
                }
            }
            throw new ArgumentException(T("Permission {0} was not found in any of the installed modules.", permissionName).ToString());
        }

        public void DeleteRole(int id) {
            _roleRepository.Delete(GetRole(id));
        }

        public IDictionary<string, IEnumerable<Permission>> GetInstalledPermissions() {
            var installedPermissions = new Dictionary<string, IEnumerable<Permission>>();
            foreach (var permissionProvider in _permissionProviders) {
                var featureName = permissionProvider.Feature.Descriptor.Id;
                var permissions = permissionProvider.GetPermissions();
                foreach(var permission in permissions) {
                    var category = permission.Category;

                    string title = String.IsNullOrWhiteSpace(category) ? T("{0} Feature", featureName).Text : T(category).Text;

                    if ( installedPermissions.ContainsKey(title) )
                        installedPermissions[title] = installedPermissions[title].Concat( new [] {permission} );
                    else
                        installedPermissions.Add(title, new[] { permission });
                }
            }

            return installedPermissions;
        }

        public IEnumerable<string> GetPermissionsForRole(int id) {
            var permissions = new List<string>();
            RoleRecord roleRecord = GetRole(id);
            foreach (RolesPermissionsRecord rolesPermission in roleRecord.RolesPermissions) {
                permissions.Add(rolesPermission.Permission.Name);
            }
            return permissions;
        }
    }
}