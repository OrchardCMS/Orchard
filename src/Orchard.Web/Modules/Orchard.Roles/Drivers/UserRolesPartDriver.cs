using System;
using System.Linq;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Roles.Events;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Roles.ViewModels;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Orchard.Roles.Drivers {
    public class UserRolesPartDriver : ContentPartDriver<UserRolesPart> {
        private readonly IRepository<UserRolesPartRecord> _userRolesRepository;
        private readonly IRoleService _roleService;
        private readonly INotifier _notifier;
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IRoleEventHandler _roleEventHandlers;
        private const string TemplateName = "Parts/Roles.UserRoles";

        public UserRolesPartDriver(
            IRepository<UserRolesPartRecord> userRolesRepository, 
            IRoleService roleService, 
            INotifier notifier,
            IAuthenticationService authenticationService,
            IAuthorizationService authorizationService, 
            IRoleEventHandler roleEventHandlers) {

            _userRolesRepository = userRolesRepository;
            _roleService = roleService;
            _notifier = notifier;
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _roleEventHandlers = roleEventHandlers;
            T = NullLocalizer.Instance;
        }

        protected override string Prefix {
            get {
                return "UserRoles";
            }
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(UserRolesPart userRolesPart, dynamic shapeHelper) {
            // don't show editor without apply roles permission
            if (!_authorizationService.TryCheckAccess(Permissions.AssignRoles, _authenticationService.GetAuthenticatedUser(), userRolesPart))
                return null;

            return ContentShape("Parts_Roles_UserRoles_Edit",
                    () => {
                       var roles =_roleService.GetRoles().Select(x => new UserRoleEntry {
                                                                          RoleId = x.Id,
                                                                          Name = x.Name,
                                                                          Granted = userRolesPart.Roles.Contains(x.Name)});
                       var model = new UserRolesViewModel {
                           User = userRolesPart.As<IUser>(),
                           UserRoles = userRolesPart,
                           Roles = roles.ToList(),
                       };
                       return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix);
                    });
        }

        protected override DriverResult Editor(UserRolesPart userRolesPart, IUpdateModel updater, dynamic shapeHelper) {
            // don't apply editor without apply roles permission
            if (!_authorizationService.TryCheckAccess(Permissions.AssignRoles, _authenticationService.GetAuthenticatedUser(), userRolesPart))
                return null;

            var model = BuildEditorViewModel(userRolesPart);
            if (updater.TryUpdateModel(model, Prefix, null, null)) {
                var currentUserRoleRecords = _userRolesRepository.Fetch(x => x.UserId == model.User.Id).ToArray();
                var currentRoleRecords = currentUserRoleRecords.Select(x => x.Role);
                var targetRoleRecords = model.Roles.Where(x => x.Granted).Select(x => _roleService.GetRole(x.RoleId)).ToArray();
                foreach (var addingRole in targetRoleRecords.Where(x => !currentRoleRecords.Contains(x))) {
                    _notifier.Warning(T("Adding role {0} to user {1}", addingRole.Name, userRolesPart.As<IUser>().UserName));
                    _userRolesRepository.Create(new UserRolesPartRecord { UserId = model.User.Id, Role = addingRole });
                    _roleEventHandlers.UserAdded(new UserAddedContext {Role = addingRole, User = model.User});
                }
                foreach (var removingRole in currentUserRoleRecords.Where(x => !targetRoleRecords.Contains(x.Role))) {
                    _notifier.Warning(T("Removing role {0} from user {1}", removingRole.Role.Name, userRolesPart.As<IUser>().UserName));
                    _userRolesRepository.Delete(removingRole);
                    _roleEventHandlers.UserRemoved(new UserRemovedContext { Role = removingRole.Role, User = model.User });
                }
            }
            return ContentShape("Parts_Roles_UserRoles_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        private static UserRolesViewModel BuildEditorViewModel(UserRolesPart userRolesPart) {
            return new UserRolesViewModel { User = userRolesPart.As<IUser>(), UserRoles = userRolesPart };
        }

        protected override void Importing(UserRolesPart part, ContentManagement.Handlers.ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "Roles", roles => {

                var userRoles = roles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                // create new roles
                foreach (var role in userRoles) {
                    var roleRecord = _roleService.GetRoleByName(role);

                    // create the role if it doesn't already exist
                    if (roleRecord == null) {
                        _roleService.CreateRole(role);
                    }
                }

                var currentUserRoleRecords = _userRolesRepository.Fetch(x => x.UserId == part.ContentItem.Id).ToList();
                var currentRoleRecords = currentUserRoleRecords.Select(x => x.Role).ToList();
                var targetRoleRecords = userRoles.Select(x => _roleService.GetRoleByName(x)).ToList();
                foreach (var addingRole in targetRoleRecords.Where(x => !currentRoleRecords.Contains(x))) {
                    _userRolesRepository.Create(new UserRolesPartRecord { UserId = part.ContentItem.Id, Role = addingRole });
                }
            });
        }

        protected override void Exporting(UserRolesPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Roles", string.Join(",", part.Roles));
        }
    }
}