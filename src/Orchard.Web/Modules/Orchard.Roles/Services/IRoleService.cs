using System.Collections.Generic;
using Orchard.Roles.Models;
using Orchard.Security.Permissions;

namespace Orchard.Roles.Services {
    public interface IRoleService : IDependency {
        IEnumerable<RoleRecord> GetRoles();
        RoleRecord GetRole(int id);
        RoleRecord GetRoleByName(string name);
        void CreateRole(string roleName);
        void CreatePermissionForRole(string roleName, string permissionName);
        void UpdateRole(int id, string roleName, IEnumerable<string> rolePermissions);
        void DeleteRole(int id);
        IDictionary<string, IEnumerable<Permission>> GetInstalledPermissions();
        IEnumerable<string> GetPermissionsForRole(int id);

        IEnumerable<string> GetPermissionsForRoleByName(string name);


        /// <summary>
        /// Verify if the role name is unique
        /// </summary>
        /// <param name="name">Role name</param>
        /// <returns>Returns false if a role with the given name already exits</returns>
        bool VerifyRoleUnicity(string name);
    }
}