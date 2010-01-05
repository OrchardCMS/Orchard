using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Orchard.Roles.Records;
using Orchard.Roles.Services;
using Orchard.Roles.ViewModels;
using Orchard.UI.Notify;

namespace Orchard.Roles.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IRoleService _roleService;
        private readonly INotifier _notifier;

        public AdminController(IRoleService roleService, INotifier notifier) {
            _roleService = roleService;
            _notifier = notifier;
        }

        public ActionResult Index() {
            var model = new RolesIndexViewModel { Rows = _roleService.GetRoles() as IList<RoleRecord> };

            return View(model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST() {
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
            var model = new RoleCreateViewModel { PackagePermissions = _roleService.GetInstalledPermissions() };
            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST() {
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
            var role = _roleService.GetRole(id);
            if (role == null) {
                //TODO: Error message
                throw new HttpException(404, "page with id " + id + " was not found");
            }
            var model = new RoleEditViewModel { Name = role.Name, Id = role.Id, 
                                                PackagePermissions = _roleService.GetInstalledPermissions(),
                                                CurrentPermissions = _roleService.GetPermissionsForRole(id)};

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST() {
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
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error("Editing Role failed: " + exception.Message);
                return RedirectToAction("Edit", viewModel.Id);
            }
        }
    }
}
