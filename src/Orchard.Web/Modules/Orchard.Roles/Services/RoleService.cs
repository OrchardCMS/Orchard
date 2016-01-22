using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Caching;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Roles.Events;
using Orchard.Roles.Models;
using Orchard.Security.Permissions;

namespace Orchard.Roles.Services {
    [UsedImplicitly]
    public class RoleService : IRoleService {
        private const string SignalName = "Orchard.Roles.Services.RoleService";

        private readonly IRepository<RoleRecord> _roleRepository;
        private readonly IRepository<PermissionRecord> _permissionRepository;
        private readonly IRepository<UserRolesPartRecord> _userRolesRepository;
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly IRoleEventHandler _roleEventHandlers;

        public RoleService(
            IRepository<RoleRecord> roleRepository,
            IRepository<PermissionRecord> permissionRepository,
            IRepository<UserRolesPartRecord> userRolesRepository,
            IEnumerable<IPermissionProvider> permissionProviders,
            ICacheManager cacheManager,
            ISignals signals, 
            IRoleEventHandler roleEventHandlers) {

            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _userRolesRepository = userRolesRepository;
            _permissionProviders = permissionProviders;
            _cacheManager = cacheManager;
            _signals = signals;
            _roleEventHandlers = roleEventHandlers;
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
            if (GetRoleByName(roleName) != null)
                return;

            var roleRecord = new RoleRecord {Name = roleName};
            _roleRepository.Create(roleRecord);
            _roleEventHandlers.Created(new RoleCreatedContext { Role = roleRecord });
            TriggerSignal();
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
            _roleEventHandlers.PermissionAdded(new PermissionAddedContext { Role = roleRecord, Permission = permissionRecord });
            TriggerSignal();
            
        }

        public void UpdateRole(int id, string roleName, IEnumerable<string> rolePermissions) {
            var roleRecord = GetRole(id);
            var currentRoleName = roleRecord.Name;
            var currentPermissions = roleRecord.RolesPermissions.ToDictionary(x => x.Permission.Name);
            roleRecord.Name = roleName;
            roleRecord.RolesPermissions.Clear();

            if (!String.Equals(currentRoleName, roleName)) {
                _roleEventHandlers.Renamed(new RoleRenamedContext {Role = roleRecord, NewRoleName = roleName, PreviousRoleName = currentRoleName});
            }

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

                if(!currentPermissions.ContainsKey(permission))
                    _roleEventHandlers.PermissionAdded(new PermissionAddedContext { Role = roleRecord, Permission = permissionRecord });
                else {
                    currentPermissions.Remove(permission);
                }
            }

            foreach(var permission in currentPermissions.Values)
                _roleEventHandlers.PermissionRemoved(new PermissionRemovedContext { Role = roleRecord, Permission = permission.Permission });

            TriggerSignal();
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

            var currentUserRoleRecords = _userRolesRepository.Fetch(x => x.Role.Id == id);
            foreach(var userRoleRecord in currentUserRoleRecords) {
                _userRolesRepository.Delete(userRoleRecord);
            }

            var roleRecord = GetRole(id);
            _roleRepository.Delete(roleRecord);
            _roleEventHandlers.Removed(new RoleRemovedContext { Role = roleRecord});
            TriggerSignal();
        }

        public IDictionary<string, IEnumerable<Permission>> GetInstalledPermissions() {
            var installedPermissions = new Dictionary<string, IEnumerable<Permission>>();
            foreach (var permissionProvider in _permissionProviders) {
                var featureName = permissionProvider.Feature.Descriptor.Id;
                var permissions = permissionProvider.GetPermissions();
                foreach (var permission in permissions) {
                    var category = permission.Category;

                    string title = String.IsNullOrWhiteSpace(category) ? T("{0} Feature", featureName).Text : T(category).Text;

                    if (installedPermissions.ContainsKey(title))
                        installedPermissions[title] = installedPermissions[title].Concat(new[] { permission });
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

        public IEnumerable<string> GetPermissionsForRoleByName(string name) {
            return _cacheManager.Get(name, ctx => {
                MonitorSignal(ctx);
                return GetPermissionsForRoleByNameInner(name).ToList();
            });
        }

        /// <summary>
        /// Verify if the role name is unique
        /// </summary>
        /// <param name="name">Role name</param>
        /// <returns>Returns false if a role with the given name already exits</returns>
        public bool VerifyRoleUnicity(string name) {
            return (_roleRepository.Get(x => x.Name == name) == null);
        }
        

        IEnumerable<string> GetPermissionsForRoleByNameInner(string name) {
            var roleRecord = GetRoleByName(name);
            return roleRecord == null ? Enumerable.Empty<string>() : GetPermissionsForRole(roleRecord.Id);
        }


        private void MonitorSignal(AcquireContext<string> ctx) {
            ctx.Monitor(_signals.When(SignalName));
        }

        private void TriggerSignal() {
            _signals.Trigger(SignalName);
        }
    }
}