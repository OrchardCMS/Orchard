using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using Orchard.Users.ViewModels;

namespace Orchard.Users.Controllers {

    public class AdminController : Controller, IUpdateModel {

        private readonly IMembershipService _membershipService;

        public AdminController(
            IOrchardServices services,
            IMembershipService membershipService) {
            Services = services;
            _membershipService = membershipService;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }


        public ActionResult Index() {
            var users = Services.ContentManager
                .Query<User, UserRecord>()
                .Where(x => x.UserName != null)
                .List();

            var model = new UsersIndexViewModel {
                Rows = users
                    .Select(x => new UsersIndexViewModel.Row { User = x })
                    .ToList()
            };

            return View(model);
        }

        public ActionResult Create() {
            var user = Services.ContentManager.New<IUser>(Models.User.ContentType.Name);
            var model = new UserCreateViewModel {
                User = Services.ContentManager.BuildEditorModel(user)
            };
            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult _Create() {

            var model = new UserCreateViewModel();
            UpdateModel(model);

            var user = _membershipService.CreateUser(new CreateUserParams(
                                              model.UserName,
                                              model.Password,
                                              model.Email,
                                              null, null, true));

            model.User = Services.ContentManager.UpdateEditorModel(user, this);

            if (model.Password != model.ConfirmPassword) {
                AddModelError("ConfirmPassword", T("Password confirmation must match"));
            }

            if (ModelState.IsValid == false) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            return RedirectToAction("edit", new { user.Id });
        }

        public ActionResult Edit(int id) {
            return View(new UserEditViewModel {
                User = Services.ContentManager.BuildEditorModel<User>(id)
            });
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult _Edit(int id) {
            var model = new UserEditViewModel {
                User = Services.ContentManager.UpdateEditorModel<User>(id, this)
            };

            // apply additional model properties that were posted on form
            UpdateModel(model);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            Services.Notifier.Information(T("User information updated"));
            return RedirectToAction("Edit", new { id });
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
        public void AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }

}
