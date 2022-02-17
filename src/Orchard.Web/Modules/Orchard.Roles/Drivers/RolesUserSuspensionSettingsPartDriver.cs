using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Roles.Constants;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Roles.ViewModels;

namespace Orchard.Roles.Drivers {
    public class RolesUserSuspensionSettingsPartDriver : ContentPartDriver<RolesUserSuspensionSettingsPart> {
        private readonly IRoleService _roleService;

        public RolesUserSuspensionSettingsPartDriver(
            IRoleService roleService) {

            _roleService = roleService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(RolesUserSuspensionSettingsPart part, dynamic shapeHelper) {

            return ContentShape("Parts_Roles_UserSuspensionSettings_Edit",
                () => {
                    var vm = BuildVM(part);
                    // check from the part what's configured.
                    return shapeHelper.EditorTemplate(
                         TemplateName: "Parts/Roles.UserSuspensionSettings",
                         Model: vm,
                         Prefix: Prefix
                         );
                }).OnGroup(T("Users").Text);
        }

        protected override DriverResult Editor(RolesUserSuspensionSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {

            var vm = BuildVM(part);
            if (updater.TryUpdateModel(vm, Prefix, null, null)) {
                part.Configuration = vm.Configuration;
            }
            return Editor(part, shapeHelper);
        }

        private RolesUserSuspensionSettingsViewModel BuildVM(RolesUserSuspensionSettingsPart part) {
            var systemRoles = SystemRoles.GetSystemRoles();
            var allRoles = _roleService
                .GetRoles()
                // exclude system roles (Anonymous and Authenticated)
                .Where(r => !systemRoles
                    .Any(sr => sr.Equals(r.Name, StringComparison.InvariantCultureIgnoreCase)))
                ;
            var vm = new RolesUserSuspensionSettingsViewModel();
            // Add the "default" element for the configuration for authenticated users
            // who have no other roles.
            vm.Configuration.Add(
                new RoleSuspensionConfiguration {
                    RoleId = 0,
                    RoleName = T("No Role").Text,
                    RoleLabel = T("Protect users with no configured role.").Text,
                    IsSafeFromSuspension = GetConfigurationStatus(part, 0)
                });
            // Add configuration elements for all existing roles.
            vm.Configuration.AddRange(allRoles
                .Select(rr => new RoleSuspensionConfiguration {
                    RoleId = rr.Id,
                    RoleName = rr.Name, // use the "current" role name from the records
                    RoleLabel = T("Protect users with the \"{0}\" user role.", rr.Name).Text,
                    IsSafeFromSuspension = GetConfigurationStatus(part, rr.Id)
                }));
            return vm;
        }

        private bool GetConfigurationStatus(RolesUserSuspensionSettingsPart part, int rId) {
            var config = part.Configuration?.FirstOrDefault(rsc => rsc.RoleId == rId);
            return config == null ? false : config.IsSafeFromSuspension;
        }
    }
}