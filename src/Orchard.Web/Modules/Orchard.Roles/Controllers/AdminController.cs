using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Core.Contents.Controllers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Roles.ViewModels;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;

namespace Orchard.Roles.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IRoleService _roleService;
        private readonly IAuthorizationService _authorizationService;

        public AdminController(
            IOrchardServices services,
            IRoleService roleService,
            INotifier notifier,
            IAuthorizationService authorizationService) {
            Services = services;
            _roleService = roleService;
            _authorizationService = authorizationService;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage roles")))
                return new HttpUnauthorizedResult();

            var model = new RolesIndexViewModel { Rows = _roleService.GetRoles().OrderBy(r => r.Name).ToList() };

            return View(model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage roles")))
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
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage roles")))
                return new HttpUnauthorizedResult();

            var model = new RoleCreateViewModel { FeaturePermissions = _roleService.GetInstalledPermissions() };
            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage roles")))
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
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage roles")))
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
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage roles")))
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

            Services.Notifier.Information(T("Your Role has been saved."));
            return RedirectToAction("Edit", new { id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Delete")]
        public ActionResult EditDeletePOST(int id) {
            return Delete(id, null);
        }

        [HttpPost]
        public ActionResult Delete(int id, string returnUrl) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage roles")))
                return new HttpUnauthorizedResult();

            _roleService.DeleteRole(id);
            Services.Notifier.Information(T("Role was successfully deleted."));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }
    }
}
