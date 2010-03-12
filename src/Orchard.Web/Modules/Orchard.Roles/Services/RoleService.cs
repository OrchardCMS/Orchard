using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Data;
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
        }

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
                    ModuleName = GetModuleName(permissionName)
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
                        ModuleName = GetModuleName(permission)
                    });
                }
                PermissionRecord permissionRecord = _permissionRepository.Get(x => x.Name == permission);
                roleRecord.RolesPermissions.Add(new RolesPermissionsRecord { Permission = permissionRecord, Role = roleRecord });
            }
        }

        private string GetModuleName(string permissionName) {
            foreach (var permissionProvider in _permissionProviders) {
                foreach (var permission in permissionProvider.GetPermissions()) {
                    if (String.Equals(permissionName, permission.Name, StringComparison.OrdinalIgnoreCase)) {
                        return permissionProvider.ModuleName;
                    }
                }
            }
            throw new ArgumentException("Permission " + permissionName + " was not found in any of the installed modules.");
        }

        private string GetPermissionDescription(string permissionName) {
            foreach (var permissionProvider in _permissionProviders) {
                foreach (var permission in permissionProvider.GetPermissions()) {
                    if (String.Equals(permissionName, permission.Name, StringComparison.OrdinalIgnoreCase)) {
                        return permission.Description;
                    }
                }
            }
            throw new ArgumentException("Permission " + permissionName + " was not found in any of the installed modules.");
        }

        public void DeleteRole(int id) {
            _roleRepository.Delete(GetRole(id));
        }

        public IDictionary<string, IEnumerable<Permission>> GetInstalledPermissions() {
            Dictionary<string, IEnumerable<Permission>> installedPermissions = new Dictionary<string, IEnumerable<Permission>>();
            foreach (var permissionProvider in _permissionProviders) {
                IEnumerable<Permission> permissions = permissionProvider.GetPermissions();
                if (installedPermissions.ContainsKey(permissionProvider.ModuleName))
                    installedPermissions[permissionProvider.ModuleName] = installedPermissions[permissionProvider.ModuleName].Concat(permissions);
                else
                    installedPermissions.Add(permissionProvider.ModuleName, permissions);
            }

            return installedPermissions;
        }

        public IEnumerable<string> GetPermissionsForRole(int id) {
            List<string> permissions = new List<string>();
            RoleRecord roleRecord = GetRole(id);
            foreach (RolesPermissionsRecord rolesPermission in roleRecord.RolesPermissions) {
                permissions.Add(rolesPermission.Permission.Name);
            }
            return permissions;
        }
    }
}