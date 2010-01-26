using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Extensions;
using Orchard.Roles.Services;
using Orchard.Security.Permissions;

namespace Orchard.Roles {
    [UsedImplicitly]
    public class Extension : ExtensionManagerEvents {
        private readonly IRoleService _roleService;
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;

        public Extension(
            IRoleService roleService,
            IEnumerable<IPermissionProvider> permissionProviders) {
            _roleService = roleService;
            _permissionProviders = permissionProviders;
        }

        public override void Enabled(ExtensionEventContext context) {
            // when another package is being enabled, locate matching permission providers
            var providersForEnabledPackage =
                _permissionProviders.Where(x => x.PackageName == context.Extension.Descriptor.Name);
            
            foreach (var permissionProvider in providersForEnabledPackage) {
                // get and iterate stereotypical groups of permissions
                var stereotypes = permissionProvider.GetDefaultStereotypes();
                foreach(var stereotype in stereotypes) {

                    // turn those stereotypes into roles
                    var role = _roleService.GetRoleByName(stereotype.Name);
                    if (role == null){
                        _roleService.CreateRole(stereotype.Name);
                        role = _roleService.GetRoleByName(stereotype.Name);
                    }

                    // and merge the stereotypical permissions into that role
                    var distinctPermissionNames = role.RolesPermissions.Select(x => x.Permission.Name)
                        .Union(stereotype.Permissions.Select(x => x.Name))
                        .Distinct();

                    _roleService.UpdateRole(role.Id, role.Name, distinctPermissionNames);
                }
            }
        }
    }
}
