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
using System.Collections.Generic;

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

            _allRoles = new Lazy<IEnumerable<RoleRecord>>(() => _roleService.GetRoles());
        }

        protected override string Prefix {
            get {
                return "UserRoles";
            }
        }

        public Localizer T { get; set; }

        private Lazy<IEnumerable<RoleRecord>> _allRoles;

        protected override DriverResult Editor(UserRolesPart userRolesPart, dynamic shapeHelper) {

            return ContentShape("Parts_Roles_UserRoles_Edit",
                () => {
                    var currentUser = _authenticationService.GetAuthenticatedUser();
                    // Get the roles we are authorized to assign
                    var authorizedRoleIds = _allRoles.Value
                        .Where(rr => _authorizationService.TryCheckAccess(
                            Permissions.CreatePermissionForAssignRole(rr.Name),
                            currentUser,
                            userRolesPart))
                        .Select(rr => rr.Id).ToList();
                    // If the user has no roles they can assign, we will show nothing
                    if (!authorizedRoleIds.Any()) {
                        return null;
                    }
                    var allRoles = _allRoles.Value
                        .Select(x => new UserRoleEntry {
                            RoleId = x.Id,
                            Name = x.Name,
                            Granted = userRolesPart.Roles.Contains(x.Name)
                        });
                    var model = new UserRolesViewModel {
                        User = userRolesPart.As<IUser>(),
                        UserRoles = userRolesPart,
                        Roles = allRoles.ToList(),
                        AuthorizedRoleIds = authorizedRoleIds
                    };
                    return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix);
                });
        }

        protected override DriverResult Editor(UserRolesPart userRolesPart, IUpdateModel updater, dynamic shapeHelper) {

            var currentUser = _authenticationService.GetAuthenticatedUser();
            // Get the roles we are authorized to assign
            var authorizedRoleIds = _allRoles.Value
                .Where(rr => _authorizationService.TryCheckAccess(
                    Permissions.CreatePermissionForAssignRole(rr.Name),
                    currentUser,
                    userRolesPart))
                .Select(rr => rr.Id).ToList();
            var model = BuildEditorViewModel(userRolesPart);
            if (updater.TryUpdateModel(model, Prefix, null, null)) {
                // We only have something to do for the roles the user is allowed to assign. We do this check
                // after the TryUpdateModel so that even if we do nothing, we'll display things as the user
                // changed them.
                if (authorizedRoleIds.Any()) {
                    // Find all RoleRecord objects for the user: these are roles that are already
                    // assigned to them.
                    var currentUserRoleRecords = _userRolesRepository.Fetch(x => x.UserId == model.User.Id).ToArray();
                    var currentRoleRecords = currentUserRoleRecords.Select(x => x.Role);
                    // The roles the user should have after the update (pending a verification that
                    // the currentUser is allowed to assign them)
                    var targetRoleRecords = model.Roles.Where(x => x.Granted).Select(x => _roleService.GetRole(x.RoleId)).ToArray();
                    foreach (var addingRole in targetRoleRecords
                        .Where(x =>
                            // user doesn't have the role yet
                            !currentRoleRecords.Contains(x)
                            // && we are authorized to assign this role
                            && authorizedRoleIds.Contains(x.Id))) {

                        _notifier.Warning(T("Adding role {0} to user {1}", addingRole.Name, userRolesPart.As<IUser>().UserName));
                        _userRolesRepository.Create(new UserRolesPartRecord { UserId = model.User.Id, Role = addingRole });
                        _roleEventHandlers.UserAdded(new UserAddedContext { Role = addingRole, User = model.User });
                    }
                    foreach (var removingRole in currentUserRoleRecords
                        .Where(x =>
                            // user has this role that they shouldn't
                            !targetRoleRecords.Contains(x.Role)
                            // && we are authorized to assign this role
                            && authorizedRoleIds.Contains(x.Role.Id))) {

                        _notifier.Warning(T("Removing role {0} from user {1}", removingRole.Role.Name, userRolesPart.As<IUser>().UserName));
                        _userRolesRepository.Delete(removingRole);
                        _roleEventHandlers.UserRemoved(new UserRemovedContext { Role = removingRole.Role, User = model.User });
                    }
                }
            }
            model.AuthorizedRoleIds = authorizedRoleIds;
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