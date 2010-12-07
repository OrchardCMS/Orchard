using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Environment;
using Orchard.Environment.Extensions.Models;
using Orchard.Logging;
using Orchard.Roles.Services;
using Orchard.Security.Permissions;

namespace Orchard.Roles {
    [UsedImplicitly]
    public class DefaultRoleUpdater : IFeatureEventHandler {
        private readonly IRoleService _roleService;
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;

        public DefaultRoleUpdater(
            IRoleService roleService,
            IEnumerable<IPermissionProvider> permissionProviders) {
            _roleService = roleService;
            _permissionProviders = permissionProviders;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        void IFeatureEventHandler.Installing(Feature feature) {
            AddDefaultRolesForFeature(feature);
        }

        void IFeatureEventHandler.Installed(Feature feature) {
        }

        void IFeatureEventHandler.Enabling(Feature feature) {
        }

        void IFeatureEventHandler.Enabled(Feature feature) {
        }

        void IFeatureEventHandler.Disabling(Feature feature) {
        }

        void IFeatureEventHandler.Disabled(Feature feature) {
        }

        void IFeatureEventHandler.Uninstalling(Feature feature) {
        }

        void IFeatureEventHandler.Uninstalled(Feature feature) {
        }

        public void AddDefaultRolesForFeature(Feature feature) {
            var featureName = feature.Descriptor.Id;

            // when another module is being enabled, locate matching permission providers
            var providersForEnabledModule = _permissionProviders.Where(x => x.Feature.Descriptor.Id == featureName);

            if (providersForEnabledModule.Any()) {
                Logger.Debug("Configuring default roles for module {0}", featureName);
            }
            else {
                Logger.Debug("No default roles for module {0}", featureName);
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
