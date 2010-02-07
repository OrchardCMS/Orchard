using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Extensions;
using Orchard.Logging;
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

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public override void Enabled(ExtensionEventContext context) {
            var extensionDisplayName = context.Extension.Descriptor.DisplayName ?? context.Extension.Descriptor.Name;

            // when another module is being enabled, locate matching permission providers
            var providersForEnabledModule =
                _permissionProviders.Where(x => x.ModuleName == extensionDisplayName);

            if (providersForEnabledModule.Any()) {
                Logger.Debug("Configuring default roles for module {0}", extensionDisplayName);
            }
            else {
                Logger.Debug("No default roles for module {0}", extensionDisplayName);
            }

            foreach (var permissionProvider in providersForEnabledModule) {
                // get and iterate stereotypical groups of permissions
                var stereotypes = permissionProvider.GetDefaultStereotypes();
                foreach (var stereotype in stereotypes) {

                    // turn those stereotypes into roles
                    var role = _roleService.GetRoleByName(stereotype.Name);
                    if (role == null) {
                        Logger.Information("Defining new role {0} for permission stereotype", stereotype.Name);

                        _roleService.CreateRole(stereotype.Name);
                        role = _roleService.GetRoleByName(stereotype.Name);
                    }

                    // and merge the stereotypical permissions into that role                    
                    var stereotypePermissionNames = (stereotype.Permissions ?? Enumerable.Empty<Permission>()).Select(x => x.Name);
                    var currentPermissionNames = role.RolesPermissions.Select(x => x.Permission.Name);

                    var distinctPermissionNames = currentPermissionNames
                        .Union(stereotypePermissionNames)
                        .Distinct();


                    // update role if set of permissions has increased
                    var additionalPermissionNames = distinctPermissionNames.Except(currentPermissionNames);

                    if (additionalPermissionNames.Any()) {
                        foreach (var permissionName in additionalPermissionNames) {
                            Logger.Information("Default role {0} granted permission {1}", stereotype.Name, permissionName);
                            _roleService.CreatePermissionForRole(role.Name, permissionName);
                        }                        
                    }
                }
            }
        }
    }
}
