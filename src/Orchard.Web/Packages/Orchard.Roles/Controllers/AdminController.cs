using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Orchard.Notify;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Roles.ViewModels;

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

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(FormCollection input) {
            try {
                foreach (string key in input.Keys) {
                    if (key.StartsWith("Checkbox.") && input[key] == "true") {
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
            var model = new RoleCreateViewModel();
            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create(FormCollection input) {
            var viewModel = new RoleCreateViewModel();
            try {
                UpdateModel(viewModel, input.ToValueProvider());
                _roleService.CreateRole(viewModel.Name);
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error("Creating Role failed: " + exception.Message);
                return View(viewModel);
            }
        }

        public ActionResult Edit(int id) {
            var role = _roleService.GetRole(id);
            if (role == null) {
                //TODO: Error message
                throw new HttpException(404, "page with id " + id + " was not found");
            }
            var model = new RoleEditViewModel { Name = role.Name, Id = role.Id };

            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(FormCollection input) {
            var viewModel = new RoleEditViewModel();
            try {
                UpdateModel(viewModel, input.ToValueProvider());
                // Save
                if (!String.IsNullOrEmpty(HttpContext.Request.Form["submit.Save"])) {
                    _roleService.UpdateRole(viewModel.Id, viewModel.Name);
                }
                else if (!String.IsNullOrEmpty(HttpContext.Request.Form["submit.Delete"])) {
                    _roleService.DeleteRole(viewModel.Id);
                }
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error("Editing media file failed: " + exception.Message);
                return View(viewModel);
            }
        }
    }
}
