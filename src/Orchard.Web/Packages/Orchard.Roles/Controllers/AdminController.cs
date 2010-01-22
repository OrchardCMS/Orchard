using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Roles.Models;
using Orchard.Roles.Records;
using Orchard.Roles.Services;
using Orchard.Roles.ViewModels;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.UI.Notify;

namespace Orchard.Roles.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IRoleService _roleService;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;

        public AdminController(
            IOrchardServices services,
            IRoleService roleService,
            INotifier notifier,
            IAuthorizationService authorizationService) {
            Services = services;
            _roleService = roleService;
            _notifier = notifier;
            _authorizationService = authorizationService;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }


        public ActionResult Index() {
            if (!Services.Authorizer.Authorize(Permissions.ManageRoles, T("Not authorized to manage roles")))
                return new HttpUnauthorizedResult();

            var model = new RolesIndexViewModel { Rows = _roleService.GetRoles() as IList<RoleRecord> };

            return View(model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST() {
            if (!Services.Authorizer.Authorize(Permissions.ManageRoles, T("Not authorized to manage roles")))
                return new HttpUnauthorizedResult();

            try {
                foreach (string key in Request.Form.Keys) {
                    if (key.StartsWith("Checkbox.") && Request.Form[key] == "true") {
                        int roleId = Convert.ToInt32(key.Substring("Checkbox.".Length));
                        _roleService.DeleteRole(roleId);
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error("Deleting Role failed: " + exception.Message);
                return View();
            }
        }

        public ActionResult Create() {
            if (!Services.Authorizer.Authorize(Permissions.ManageRoles, T("Not authorized to manage roles")))
                return new HttpUnauthorizedResult();

            var model = new RoleCreateViewModel { PackagePermissions = _roleService.GetInstalledPermissions() };
            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST() {
            if (!Services.Authorizer.Authorize(Permissions.ManageRoles, T("Not authorized to manage roles")))
                return new HttpUnauthorizedResult();

            var viewModel = new RoleCreateViewModel();
            try {
                UpdateModel(viewModel);
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
            catch (Exception exception) {
                _notifier.Error("Creating Role failed: " + exception.Message);
                return RedirectToAction("Create");
            }
        }

        public ActionResult Edit(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageRoles, T("Not authorized to manage roles")))
                return new HttpUnauthorizedResult();

            var role = _roleService.GetRole(id);
            if (role == null) {
                //TODO: Error message
                throw new HttpException(404, "page with id " + id + " was not found");
            }
            var model = new RoleEditViewModel { Name = role.Name, Id = role.Id, 
                                                PackagePermissions = _roleService.GetInstalledPermissions(),
                                                CurrentPermissions = _roleService.GetPermissionsForRole(id)};

            var simulation = UserSimulation.Create(role.Name);
            model.EffectivePermissions = model.PackagePermissions
                .SelectMany(group => group.Value)
                .Where(permission => _authorizationService.CheckAccess(simulation, permission))
                .Select(permission=>permission.Name)
                .Distinct()
                .ToList();

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST() {
            if (!Services.Authorizer.Authorize(Permissions.ManageRoles, T("Not authorized to manage roles")))
                return new HttpUnauthorizedResult();

            var viewModel = new RoleEditViewModel();
            try {
                UpdateModel(viewModel);
                // Save
                if (!String.IsNullOrEmpty(HttpContext.Request.Form["submit.Save"])) {
                    List<string> rolePermissions = new List<string>();
                    foreach (string key in Request.Form.Keys) {
                        if (key.StartsWith("Checkbox.") && Request.Form[key] == "true") {
                            string permissionName = key.Substring("Checkbox.".Length);
                            rolePermissions.Add(permissionName);
                        }
                    }
                    _roleService.UpdateRole(viewModel.Id, viewModel.Name, rolePermissions);
                }
                else if (!String.IsNullOrEmpty(HttpContext.Request.Form["submit.Delete"])) {
                    _roleService.DeleteRole(viewModel.Id);
                }
                return RedirectToAction("Edit", new { viewModel.Id });
            }
            catch (Exception exception) {
                _notifier.Error("Editing Role failed: " + exception.Message);
                return RedirectToAction("Edit", viewModel.Id);
            }
        }
    }
}
