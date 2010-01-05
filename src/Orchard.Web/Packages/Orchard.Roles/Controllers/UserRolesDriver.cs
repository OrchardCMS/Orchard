using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.ViewModels;
using Orchard.Roles.Models.NoRecord;
using Orchard.Roles.Records;
using Orchard.Roles.Services;
using Orchard.Roles.ViewModels;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Orchard.Roles.Controllers {
    public class UserRolesDriver : ContentPartDriver<UserRoles> {
        private readonly IRepository<UserRolesRecord> _userRolesRepository;
        private readonly IRoleService _roleService;
        private readonly INotifier _notifier;

        public UserRolesDriver(
            IRepository<UserRolesRecord> userRolesRepository, 
            IRoleService roleService, 
            INotifier notifier) {
            _userRolesRepository = userRolesRepository;
            _roleService = roleService;
            _notifier = notifier;
        }

        protected override string Prefix {
            get {
                return "UserRoles";
            }
        }

        protected override DriverResult Editor(UserRoles userRoles) {
            var roles =
                    _roleService.GetRoles().Select(
                        x => new UserRoleEntry {
                            RoleId = x.Id,
                            Name = x.Name,
                            Granted = userRoles.Roles.Contains(x.Name)
                        });

            var model = new UserRolesViewModel {
                User = userRoles.As<IUser>(),
                UserRoles = userRoles,
                Roles = roles.ToList(),
            };
            return ContentPartTemplate(model, "Parts/Roles.UserRoles");
        }

        protected override DriverResult Editor(UserRoles userRoles, IUpdateModel updater) {
            var model = new UserRolesViewModel {
                User = userRoles.As<IUser>(),
                UserRoles = userRoles,
            };

            if (updater.TryUpdateModel(model, Prefix, null, null)) {

                var currentUserRoleRecords = _userRolesRepository.Fetch(x => x.UserId == model.User.Id);
                var currentRoleRecords = currentUserRoleRecords.Select(x => x.Role);
                var targetRoleRecords = model.Roles.Where(x => x.Granted).Select(x => _roleService.GetRole(x.RoleId));

                foreach (var addingRole in targetRoleRecords.Where(x => !currentRoleRecords.Contains(x))) {
                    _notifier.Warning(string.Format("Adding role {0} to user {1}", addingRole.Name, userRoles.As<IUser>().UserName));
                    _userRolesRepository.Create(new UserRolesRecord { UserId = model.User.Id, Role = addingRole });
                }

                foreach (var removingRole in currentUserRoleRecords.Where(x => !targetRoleRecords.Contains(x.Role))) {
                    _notifier.Warning(string.Format("Removing role {0} from user {1}", removingRole.Role.Name, userRoles.As<IUser>().UserName));
                    _userRolesRepository.Delete(removingRole);
                }

            }
            return ContentPartTemplate(model, "Parts/Roles.UserRoles");
        }
    }
}
