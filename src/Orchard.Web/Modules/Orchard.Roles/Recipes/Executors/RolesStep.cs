using System;
using System.Linq;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Roles.Services;

namespace Orchard.Roles.Recipes.Executors {
    public class RolesStep : RecipeExecutionStep {
        private readonly IRoleService _roleService;

        public RolesStep(
            IRoleService roleService,
            RecipeExecutionLogger logger) : base(logger) {

            _roleService = roleService;
        }

        public override string Name {
            get { return "Roles"; }
        }

        public override void Execute(RecipeExecutionContext context) {
            var installedPermissions = _roleService.GetInstalledPermissions().SelectMany(p => p.Value).ToList();

            foreach (var roleElement in context.RecipeStep.Step.Elements()) {
                var roleName = roleElement.Attribute("Name").Value;

                Logger.Information("Importing role '{0}'.", roleName);

                try {
                    var role = _roleService.GetRoleByName(roleName);
                    if (role == null) {
                        _roleService.CreateRole(roleName);
                        role = _roleService.GetRoleByName(roleName);
                    }

                    var permissions = roleElement.Attribute("Permissions").Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    // Only import permissions for currenlty installed modules.
                    var permissionsValid = permissions.Where(permission => installedPermissions.Any(x => x.Name == permission)).ToList();

                    // Union to keep existing permissions.
                    _roleService.UpdateRole(role.Id, role.Name, permissionsValid.Union(role.RolesPermissions.Select(p => p.Permission.Name)));
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error while importing role '{0}'.", roleName);
                    throw;
                }
            }
        }
    }
}