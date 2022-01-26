using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Roles.Events;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Roles.ViewModels;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Orchard.Roles.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IRoleService _roleService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IContentManager _contentManager;
        private readonly IRepository<UserRolesPartRecord> _userRolesRepository;
        private readonly INotifier _notifier;
        private readonly IRoleEventHandler _roleEventHandlers;

        public AdminController(
            IOrchardServices services,
            IRoleService roleService,
            INotifier notifier,
            IAuthorizationService authorizationService,
            IWorkContextAccessor workContextAccessor,
            IContentManager contentManager,
            IShapeFactory shapeFactory,
            IRepository<UserRolesPartRecord> userRolesRepository,
            IRoleEventHandler roleEventHandlers) {

            Services = services;
            _roleService = roleService;
            _authorizationService = authorizationService;
            _workContextAccessor = workContextAccessor;
            _contentManager = contentManager;
            _userRolesRepository = userRolesRepository;
            _notifier = notifier;
            _roleEventHandlers = roleEventHandlers;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index() {
            if (!Services.Authorizer.Authorize(Permissions.ManageRoles, T("Not authorized to manage roles")))
                return new HttpUnauthorizedResult();

            var model = new RolesIndexViewModel { Rows = _roleService.GetRoles().OrderBy(r => r.Name).ToList() };

            return View(model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST() {
            if (!Services.Authorizer.Authorize(Permissions.ManageRoles, T("Not authorized to manage roles")))
                return new HttpUnauthorizedResult();

            foreach (string key in Request.Form.Keys) {
                if (key.StartsWith("Checkbox.") && Request.Form[key] == "true") {
                    int roleId = Convert.ToInt32(key.Substring("Checkbox.".Length));
                    _roleService.DeleteRole(roleId);
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Create() {
            if (!Services.Authorizer.Authorize(Permissions.ManageRoles, T("Not authorized to manage roles")))
                return new HttpUnauthorizedResult();

            var model = new RoleCreateViewModel { FeaturePermissions = _roleService.GetInstalledPermissions() };
            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST() {
            if (!Services.Authorizer.Authorize(Permissions.ManageRoles, T("Not authorized to manage roles")))
                return new HttpUnauthorizedResult();

            var viewModel = new RoleCreateViewModel();
            TryUpdateModel(viewModel);

            if(String.IsNullOrEmpty(viewModel.Name)) {
                ModelState.AddModelError("Name", T("Role name can't be empty"));
            }

            var role = _roleService.GetRoleByName(viewModel.Name);
            if (role != null) {
                ModelState.AddModelError("Name", T("Role with same name already exists"));
            }

            if (!ModelState.IsValid) {
                viewModel.FeaturePermissions = _roleService.GetInstalledPermissions();
                return View(viewModel);
            }

            _roleService.CreateRole(viewModel.Name);
            foreach (string key in Request.Form.Keys) {
                if (key.StartsWith("Checkbox.") && Request.Form[key] == "true") {
                    string permissionName = key.Substring("Checkbox.".Length);
                    _roleService.CreatePermissionForRole(viewModel.Name,
                                                            permissionName);
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageRoles, T("Not authorized to manage roles")))
                return new HttpUnauthorizedResult();

            var role = _roleService.GetRole(id);
            if (role == null) {
                return HttpNotFound();
            }

            var model = new RoleEditViewModel { Name = role.Name, Id = role.Id, 
                                                RoleCategoryPermissions = _roleService.GetInstalledPermissions(),
                                                CurrentPermissions = _roleService.GetPermissionsForRole(id)};

            var simulation = UserSimulation.Create(role.Name);
            model.EffectivePermissions = model.RoleCategoryPermissions
                .SelectMany(group => group.Value)
                .Where(permission => _authorizationService.TryCheckAccess(permission, simulation, null))
                .Select(permission=>permission.Name)
                .Distinct()
                .ToList();

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditSavePOST(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageRoles, T("Not authorized to manage roles")))
                return new HttpUnauthorizedResult();

            var viewModel = new RoleEditViewModel();
            TryUpdateModel(viewModel);

            if (String.IsNullOrEmpty(viewModel.Name)) {
                ModelState.AddModelError("Name", T("Role name can't be empty"));
            }

            var role = _roleService.GetRoleByName(viewModel.Name);
            if (role != null && role.Id != id) {
                ModelState.AddModelError("Name", T("Role with same name already exists"));
            }

            if (!ModelState.IsValid) {
                return Edit(id);
            }

            // Save
            List<string> rolePermissions = new List<string>();
            foreach (string key in Request.Form.Keys) {
                if (key.StartsWith("Checkbox.") && Request.Form[key] == "true") {
                    string permissionName = key.Substring("Checkbox.".Length);
                    rolePermissions.Add(permissionName);
                }
            }
            _roleService.UpdateRole(viewModel.Id, viewModel.Name, rolePermissions);

            Services.Notifier.Success(T("Your Role has been saved."));
            return RedirectToAction("Edit", new { id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Delete")]
        public ActionResult EditDeletePOST(int id) {
            return Delete(id, null);
        }

        [HttpPost]
        public ActionResult Delete(int id, string returnUrl) {
            if (!Services.Authorizer.Authorize(Permissions.ManageRoles, T("Not authorized to manage roles")))
                return new HttpUnauthorizedResult();

            _roleService.DeleteRole(id);
            Services.Notifier.Success(T("Role was successfully deleted."));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        [Authorize]
        public ActionResult Assign(int id) {
            // CurrentUser is trying to access a page to assign roles to the User
            // with Id id.
            var currentUser = _workContextAccessor.GetContext().CurrentUser;
            // Get the user whose roles we want to assign
            var userRolesPart = _contentManager.Get<UserRolesPart>(id);
            if (userRolesPart == null) {
                return HttpNotFound();
            }
            // Check whether the current user has any of the required permissions
            var allRoles = _roleService.GetRoles();
            var authorizedRoleIds = allRoles
                .Where(rr => _authorizationService.TryCheckAccess(
                    Permissions.CreatePermissionForAssignRole(rr.Name),
                    currentUser,
                    userRolesPart))
                .Select(rr => rr.Id).ToList();
            if (!authorizedRoleIds.Any()) {
                return new HttpUnauthorizedResult();
            }
            // create the ViewModel used to manage a user's roles
            var model = new UserRolesViewModel {
                User = userRolesPart.As<IUser>(),
                UserRoles = userRolesPart,
                Roles = allRoles.Select(x => new UserRoleEntry {
                    RoleId = x.Id,
                    Name = x.Name,
                    Granted = userRolesPart.Roles.Contains(x.Name)
                }).ToList(),
                AuthorizedRoleIds = authorizedRoleIds
            };

            // this calls the same view used by the driver that lets users with higher
            // permissions do the same.
            return AssignView(model);
        }

        [HttpPost, ActionName("Assign"), Authorize]
        public ActionResult AssignPOST(int id, string returnUrl) {
            var currentUser = _workContextAccessor.GetContext().CurrentUser;
            // Get the user whose roles we want to assign
            var userRolesPart = _contentManager.Get<UserRolesPart>(id);
            if (userRolesPart == null) {
                return HttpNotFound();
            }
            // Check whether the current user has any of the required permissions
            var allRoles = _roleService.GetRoles();
            var authorizedRoleIds = allRoles
                .Where(rr => _authorizationService.TryCheckAccess(
                    Permissions.CreatePermissionForAssignRole(rr.Name),
                    currentUser,
                    userRolesPart))
                .Select(rr => rr.Id).ToList();
            if (!authorizedRoleIds.Any()) {
                return new HttpUnauthorizedResult();
            }
            // Start trying to update
            var editModel = new UserRolesViewModel {
                User = userRolesPart.As<IUser>(),
                UserRoles = userRolesPart
            };
            if (TryUpdateModel(editModel)) {
                // same logic that is used in the UserRolesPartDriver:
                var currentUserRoleRecords = _userRolesRepository.Fetch(x => x.UserId == editModel.User.Id).ToArray();
                var currentRoleRecords = currentUserRoleRecords.Select(x => x.Role);
                // The roles the user should have after the update (pending a verification that
                // the currentUser is allowed to assign them)
                var targetRoleRecords = editModel.Roles.Where(x => x.Granted).Select(x => _roleService.GetRole(x.RoleId)).ToArray();
                foreach (var addingRole in targetRoleRecords
                    .Where(x =>
                        // user doesn't have the role yet
                        !currentRoleRecords.Contains(x)
                        // && we are authorized to assign this role
                        && authorizedRoleIds.Contains(x.Id))) {

                    _notifier.Warning(T("Adding role {0} to user {1}", addingRole.Name, userRolesPart.As<IUser>().UserName));
                    _userRolesRepository.Create(new UserRolesPartRecord { UserId = editModel.User.Id, Role = addingRole });
                    _roleEventHandlers.UserAdded(new UserAddedContext { Role = addingRole, User = editModel.User });
                }
                foreach (var removingRole in currentUserRoleRecords
                    .Where(x =>
                        // user has this role that they shouldn't
                        !targetRoleRecords.Contains(x.Role)
                        // && we are authorized to assign this role
                        && authorizedRoleIds.Contains(x.Role.Id))) {

                    _notifier.Warning(T("Removing role {0} from user {1}", removingRole.Role.Name, userRolesPart.As<IUser>().UserName));
                    _userRolesRepository.Delete(removingRole);
                    _roleEventHandlers.UserRemoved(new UserRemovedContext { Role = removingRole.Role, User = editModel.User });
                }
            }

            if (!ModelState.IsValid) {
                editModel.AuthorizedRoleIds = authorizedRoleIds;
                // Something went wrong in the update
                Services.TransactionManager.Cancel();
                return AssignView(editModel);
            }
            return this.RedirectLocal(returnUrl, () => RedirectToAction("Assign", new { id = id }));
        }

        private ActionResult AssignView(UserRolesViewModel editModel) {

            var editor = Shape.EditorTemplate(
                TemplateName: "Parts/Roles.UserRoles",
                Model: editModel,
                Prefix: null);

            return View(editor
                .UserName(editModel.User?.UserName));
        }
    }
}
