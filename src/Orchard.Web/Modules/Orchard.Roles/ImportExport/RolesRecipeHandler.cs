using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Roles.Models;
using Orchard.Roles.Services;

namespace Orchard.Roles.ImportExport {
    public class RolesRecipeHandler : IRecipeHandler {
        private readonly IRoleService _roleService;
        private readonly IRepository<RoleRecord> _roleRecordRepository;
        private readonly IRepository<PermissionRecord> _permissionRepository;

        public RolesRecipeHandler(IRoleService roleService, 
            IRepository<RoleRecord> roleRecordRepository, 
            IRepository<PermissionRecord> permissionRepository) {
            _roleService = roleService;
            _roleRecordRepository = roleRecordRepository;
            _permissionRepository = permissionRepository;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Roles", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            Logger.Information("Executing recipe step '{0}'; ExecutionId={1}", recipeContext.RecipeStep.Name, recipeContext.ExecutionId);

            var installedPermissions = _roleService.GetInstalledPermissions().SelectMany(p => p.Value).ToList();

            foreach (var roleElement in recipeContext.RecipeStep.Step.Elements()) {
                var roleName = roleElement.Attribute("Name").Value;

                Logger.Information("Processing role '{0}'.", roleName);

                try {
                    var role = _roleService.GetRoleByName(roleName);
                    if (role == null) {
                        _roleService.CreateRole(roleName);
                        role = _roleService.GetRoleByName(roleName);
                    }

                    var permissions = roleElement.Attribute("Permissions").Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    // only import permissions for currenlty installed modules
                    var permissionsValid = permissions.Where(permission => installedPermissions.Any(x => x.Name == permission)).ToList();

                    // union to keep existing permissions
                    _roleService.UpdateRole(role.Id, role.Name, permissionsValid.Union(role.RolesPermissions.Select(p => p.Permission.Name)));
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error while processing role '{0}'.", roleName);
                    throw;
                }
            }

            recipeContext.Executed = true;
            Logger.Information("Finished executing recipe step '{0}'.", recipeContext.RecipeStep.Name);
        }
    }
}