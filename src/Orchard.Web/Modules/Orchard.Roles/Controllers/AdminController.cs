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

            var model = new RolesIndexViewModel { Rows = _roleService.GetRoles().ToList() };

            return View(model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage roles")))
                return new HttpUnauthorizedResult();

            try {
                foreach (string key in Request.Form.Keys) {
                    if (key.StartsWith("Checkbox.") && Request.Form[key] == "true") {
                        int roleId = Convert.ToInt32(key.Substring("Checkbox.".Length));
                        _roleService.DeleteRole(roleId);
                    }
                }
                return RedirectToAction("Index");
            } catch (Exception exception) {
                this.Error(exception, T("Deleting Role failed: {0}", exception.Message), Logger, Services.Notifier);

                return View();
            }
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
            try {
                UpdateModel(viewModel);

                //check if the role name already exists
                if (!_roleService.VerifyRoleUnicity(viewModel.Name)) {
                    Services.Notifier.Error(T("Creating Role {0} failed: Role with same name already exists", viewModel.Name));
                    return RedirectToAction("Create");
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
            } catch (Exception exception) {
                this.Error(exception, T("Creating Role failed: {0}", exception.Message), Logger, Services.Notifier);

                return RedirectToAction("Create");
            }
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
            try {
                UpdateModel(viewModel);
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
            } catch (Exception exception) {
                this.Error(exception, T("Editing Role failed: {0}", exception.Message), Logger, Services.Notifier);

                return RedirectToAction("Edit", id);
            }
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

            try {
                _roleService.DeleteRole(id);

                Services.Notifier.Information(T("Role was successfully deleted."));

                return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
            } catch (Exception exception) {
                this.Error(exception, T("Editing Role failed: {0}", exception.Message), Logger, Services.Notifier);

                return RedirectToAction("Edit", id);
            }
        }
    }
}
