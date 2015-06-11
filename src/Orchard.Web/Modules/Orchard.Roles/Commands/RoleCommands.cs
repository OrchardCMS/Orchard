using System.Collections.Generic;
using System.Linq;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Security;

namespace Orchard.Roles.Commands {
    public class RoleCommands : DefaultOrchardCommandHandler {
        private readonly IRoleService _roleService;
        private readonly IMembershipService _membershipService;
        private readonly IRepository<UserRolesPartRecord> _userRolesRepository;
        private readonly IContentManager _contentManager;

        public RoleCommands(
            IRoleService roleService, 
            IMembershipService membershipService, 
            IRepository<UserRolesPartRecord> userRolesRepository,
            IContentManager contentManager) {
            _roleService = roleService;
            _membershipService = membershipService;
            _userRolesRepository = userRolesRepository;
            _contentManager = contentManager;
        }

        [OrchardSwitch]
        public string WithFeature { get; set; }

        [OrchardSwitch]
        public string WithPermission { get; set; }

        [OrchardSwitch]
        public bool IncludeUsers { get; set; }

        [OrchardSwitch]
        public bool IncludePermissions { get; set; }

        [CommandHelp("role list [/WithFeature:\"feature\"] [/WithPermission:permission] [/IncludeUsers:true|false] [/IncludePermissions:true|false]\r\n\t" + "Lists all roles by name")]
        [CommandName("role list")]
        [OrchardSwitches("WithFeature,WithPermission,IncludeUsers,IncludePermissions")]
        public void RoleList() {
            var roleRecords = _roleService.GetRoles().OrderBy(record => record.Name);

            Context.Output.WriteLine(T("List of Roles"));
            Context.Output.WriteLine(T("--------------------------"));

            foreach (var roleRecord in roleRecords) {
                if (WithPermission != null) {
                    if (roleRecord.RolesPermissions.All(record => record.Permission.Name != WithPermission)) {
                        continue;
                    }
                }

                if (WithFeature != null) {
                    if (roleRecord.RolesPermissions.All(record => record.Permission.FeatureName != WithFeature)) {
                        continue;
                    }
                }

                PrintRoleRecord(roleRecord, 2);

                if (IncludePermissions || IncludeUsers)
                    Context.Output.WriteLine();
            }
        }

        [CommandHelp("role detail <name> [/WithFeature:\"feature\"] [/WithPermission:permission] [/IncludeUsers:true|false] [/IncludePermissions:true|false]\r\n\t" + "Displays Role Details")]
        [CommandName("role detail")]
        [OrchardSwitches("WithFeature,WithPermission,IncludeUsers,IncludePermissions")]
        public void RoleDetail(string name) {
            var role = _roleService.GetRoleByName(name);
            PrintRoleRecord(role);
        }

        private void PrintRoleRecord(RoleRecord roleRecord, int initialIndent = 0) {
            var secondIndent = initialIndent + 2;

            Context.Output.Write(new string(' ', initialIndent));
            Context.Output.WriteLine(T("{0}", roleRecord.Name));

            if (IncludePermissions) {
                Context.Output.Write(new string(' ', secondIndent));
                Context.Output.WriteLine(T("List of Permissions"));

                Context.Output.Write(new string(' ', secondIndent));
                Context.Output.WriteLine(T("--------------------------"));

                var permissionsEnumerable =
                    roleRecord.RolesPermissions
                        .Where(record => WithFeature == null || record.Permission.FeatureName == WithFeature)
                        .Where(record => WithPermission == null || record.Permission.Name == WithPermission);

                var orderedPermissionsEnumerable =
                    permissionsEnumerable
                        .OrderBy(record => record.Permission.FeatureName)
                        .ThenBy(record => record.Permission.Name);

                foreach (var rolesPermissionsRecord in orderedPermissionsEnumerable) {
                    Context.Output.Write(new string(' ', secondIndent));
                    Context.Output.Write("Feature Name:".PadRight(15));
                    Context.Output.WriteLine(rolesPermissionsRecord.Permission.FeatureName);

                    Context.Output.Write(new string(' ', secondIndent));
                    Context.Output.Write("Permission:".PadRight(15));
                    Context.Output.WriteLine(rolesPermissionsRecord.Permission.Name);

                    Context.Output.Write(new string(' ', secondIndent));
                    Context.Output.Write("Description:".PadRight(15));
                    Context.Output.WriteLine(rolesPermissionsRecord.Permission.Description);
                    Context.Output.WriteLine();
                }
            }

            if (IncludeUsers) {
                var userRolesPartRecords = _userRolesRepository.Fetch(record => record.Role.Name == roleRecord.Name);

                Context.Output.Write(new string(' ', secondIndent));
                Context.Output.WriteLine(T("List of Users"));

                Context.Output.Write(new string(' ', secondIndent));
                Context.Output.WriteLine(T("--------------------------"));

                foreach (var userRolesPartRecord in userRolesPartRecords) {
                    var userRolesPart = _contentManager.Get<UserRolesPart>(userRolesPartRecord.UserId);
                    var user = userRolesPart.As<IUser>();
                    
                    Context.Output.Write(new string(' ', secondIndent));
                    Context.Output.Write("UserName:".PadRight(15));
                    Context.Output.WriteLine(user.UserName);

                    Context.Output.Write(new string(' ', secondIndent));
                    Context.Output.Write("Email:".PadRight(15));
                    Context.Output.WriteLine(user.Email);
                    Context.Output.WriteLine();
                }
            }
        }

        [CommandHelp("permission list [/WithFeature:\"feature\"]\r\n\t" + "Lists Permissions")]
        [CommandName("permission list")]
        [OrchardSwitches("WithFeature")]
        public void PermissionList() {
            var installedPermissions = _roleService.GetInstalledPermissions();

            IEnumerable<string> featureNames;
            if (WithFeature == null) {
                featureNames = installedPermissions.Keys.OrderBy(s => s);
            }
            else {
                var matchedFeature = installedPermissions.Keys.FirstOrDefault(s => s == WithFeature || s == string.Format("{0} Feature", WithFeature));
                if (matchedFeature == null) {
                    Context.Output.WriteLine("Feature '{0}' is not found", WithFeature);
                    return;
                }

                featureNames = new[] {matchedFeature};
            }

            Context.Output.WriteLine(T("List of Permissions"));
            Context.Output.WriteLine(T("--------------------------"));

            const int firstIndent = 2;
            const int secondIndent = 4;

            foreach (var featureName in featureNames) {
                Context.Output.Write(new string(' ', firstIndent));
                Context.Output.Write("Feature:".PadRight(8));
                Context.Output.WriteLine(featureName);

                foreach (var permission in installedPermissions[featureName].OrderBy(permission => permission.Name)) {
                    if (permission.Category != null) {
                        Context.Output.Write(new string(' ', secondIndent));
                        Context.Output.Write("Category:".PadRight(15));
                        Context.Output.WriteLine(permission.Category);
                    }

                    Context.Output.Write(new string(' ', secondIndent));
                    Context.Output.Write("Permission:".PadRight(15));
                    Context.Output.WriteLine(permission.Name);

                    Context.Output.Write(new string(' ', secondIndent));
                    Context.Output.Write("Description:".PadRight(15));
                    Context.Output.WriteLine(permission.Description);

                    Context.Output.WriteLine();
                }

                Context.Output.WriteLine();
            }
        }

        [CommandHelp("user roles <username>\r\n\t" + "Lists a User's Roles")]
        [CommandName("user roles")]
        public void GetUserRoles(string username) {
            var user = _membershipService.GetUser(username);

            if (user == null) {
                Context.Output.WriteLine("Username not found");
                return;
            }

            Context.Output.WriteLine(T("List of Roles"));
            Context.Output.WriteLine(T("--------------------------"));

            foreach (var role in user.As<UserRolesPart>().Roles) {
                Context.Output.Write(new string(' ', 2));
                Context.Output.WriteLine(role);
            }
        }

        [CommandHelp("user add role <username> <role>\r\n\t" + "Adds a User to a Role")]
        [CommandName("user add role")]
        public void UserAddRole(string username, string role) {
            var user = _membershipService.GetUser(username);

            if (user == null) {
                Context.Output.WriteLine("User not found");
                return;
            }

            var roleRecord = _roleService.GetRoleByName(role);
            if (roleRecord == null) {
                Context.Output.WriteLine("Role not found");
                return;
            }

            var existingAssociation = _userRolesRepository.Get(record => record.UserId == user.Id && record.Role.Id == roleRecord.Id);
            if (existingAssociation != null)
                return;

            Context.Output.WriteLine(T("Adding role {0} to user {1}", roleRecord.Name, user.UserName));
            _userRolesRepository.Create(new UserRolesPartRecord { Role = roleRecord, UserId = user.Id });
        }

        [CommandHelp("user remove role <username> <role>\r\n\t" + "Removes a User from a Role")]
        [CommandName("user remove role")]
        public void UserRemoveRole(string username, string role) {
            var user = _membershipService.GetUser(username);

            if (user == null) {
                Context.Output.WriteLine("User not found");
                return;
            }

            var roleRecord = _roleService.GetRoleByName(role);
            if (roleRecord == null) {
                Context.Output.WriteLine("Role not found");
                return;
            }

            var existingAssociation = _userRolesRepository.Get(record => record.UserId == user.Id && record.Role.Id == roleRecord.Id);
            if (existingAssociation == null)
                return;

            Context.Output.WriteLine(T("Removing role {0} from user {1}", roleRecord.Name, user.UserName));
            _userRolesRepository.Delete(existingAssociation);
        }

        [CommandHelp("role add permission <role> <permission>\r\n\t" + "Adds a Permission to a Role")]
        [CommandName("role add permission")]
        [OrchardSwitches("Force")]
        public void RoleAddPermission(string role, string addPermission) {
            var roleRecord = _roleService.GetRoleByName(role);
            if (roleRecord == null) {
                Context.Output.WriteLine("Role not found");
                return;
            }

            var currentPermissions = _roleService.GetPermissionsForRole(roleRecord.Id).ToList();
            if (currentPermissions.Contains(addPermission))
                return;

            Context.Output.WriteLine(T("Adding permission {0} to role {1}", addPermission, role));

            currentPermissions.Add(addPermission);
            _roleService.UpdateRole(roleRecord.Id, roleRecord.Name, currentPermissions);
        }

        [CommandHelp("role remove permission <role> <permission>\r\n\t" + "Removes a Permission from a Role")]
        [CommandName("role remove permission")]
        public void RoleRemovePermission(string role, string removePermission) {
            var roleRecord = _roleService.GetRoleByName(role);
            if (roleRecord == null) {
                Context.Output.WriteLine("Role not found");
                return;
            }

            var currentPermissions = _roleService.GetPermissionsForRole(roleRecord.Id).ToList();
            if (!currentPermissions.Contains(removePermission))
                return;

            Context.Output.WriteLine(T("Removing permission {0} from role {1}", removePermission, role));

            currentPermissions.Remove(removePermission);
            _roleService.UpdateRole(roleRecord.Id, roleRecord.Name, currentPermissions);
        }

        [CommandHelp("role create <role>\r\n\t" + "Creates a Role")]
        [CommandName("role create")]
        public void RoleCreate(string role) {
            var existingRole = _roleService.GetRoleByName(role);
            if (existingRole != null) {
                Context.Output.WriteLine(T("Role {0} already exists", role));
                return;
            }

            Context.Output.WriteLine(T("Creating role {0}", role));
            _roleService.CreateRole(role);
        }

        [CommandHelp("role delete <role>\r\n\t" + "Deletes a Role")]
        [CommandName("role delete")]
        public void RoleDelete(string role) {
            var existingRole = _roleService.GetRoleByName(role);
            if (existingRole == null) {
                Context.Output.WriteLine(T("Role {0} doesn't exist", role));
                return;
            }

            Context.Output.WriteLine(T("Deleting role {0}", role));
            _roleService.DeleteRole(existingRole.Id);
        }

    }
}