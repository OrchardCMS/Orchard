using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Roles.ViewModels;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Orchard.Roles.Drivers {
    [UsedImplicitly]
    public class UserRolesDriver : ContentPartDriver<UserRoles> {
        private readonly IRepository<UserRolesRecord> _userRolesRepository;
        private readonly IRoleService _roleService;
        private readonly INotifier _notifier;
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;

        public UserRolesDriver(
            IRepository<UserRolesRecord> userRolesRepository, 
            IRoleService roleService, 
            INotifier notifier,
            IAuthenticationService authenticationService,
            IAuthorizationService authorizationService) {
            _userRolesRepository = userRolesRepository;
            _roleService = roleService;
            _notifier = notifier;
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            T = NullLocalizer.Instance;
        }

        protected override string Prefix {
            get {
                return "UserRoles";
            }
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(UserRoles userRoles) {
            // don't show editor without apply roles permission
            if (!_authorizationService.TryCheckAccess(Permissions.ApplyRoles, _authenticationService.GetAuthenticatedUser(), userRoles))
                return null;

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
            // don't apply editor without apply roles permission
            if (!_authorizationService.TryCheckAccess(Permissions.ApplyRoles, _authenticationService.GetAuthenticatedUser(), userRoles))
                return null;

            var model = new UserRolesViewModel {
                                                   User = userRoles.As<IUser>(),
                                                   UserRoles = userRoles,
                                               };

            if (updater.TryUpdateModel(model, Prefix, null, null)) {

                var currentUserRoleRecords = _userRolesRepository.Fetch(x => x.UserId == model.User.Id);
                var currentRoleRecords = currentUserRoleRecords.Select(x => x.Role);
                var targetRoleRecords = model.Roles.Where(x => x.Granted).Select(x => _roleService.GetRole(x.RoleId));

                foreach (var addingRole in targetRoleRecords.Where(x => !currentRoleRecords.Contains(x))) {
                    _notifier.Warning(T("Adding role {0} to user {1}", addingRole.Name, userRoles.As<IUser>().UserName));
                    _userRolesRepository.Create(new UserRolesRecord { UserId = model.User.Id, Role = addingRole });
                }

                foreach (var removingRole in currentUserRoleRecords.Where(x => !targetRoleRecords.Contains(x.Role))) {
                    _notifier.Warning(T("Removing role {0} from user {1}", removingRole.Role.Name, userRoles.As<IUser>().UserName));
                    _userRolesRepository.Delete(removingRole);
                }

            }
            return ContentPartTemplate(model, "Parts/Roles.UserRoles");
        }
    }
}