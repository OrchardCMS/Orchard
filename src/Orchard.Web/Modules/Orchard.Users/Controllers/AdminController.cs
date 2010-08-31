using System.Linq;
using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Notify;
using Orchard.Users.Drivers;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Users.ViewModels;

namespace Orchard.Users.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly IMembershipService _membershipService;
        private readonly IUserService _userService;

        public AdminController(
            IOrchardServices services,
            IMembershipService membershipService,
            IUserService userService) {
            Services = services;
            _membershipService = membershipService;
            _userService = userService;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }

        public ActionResult Index() {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to list users")))
                return new HttpUnauthorizedResult();

            var users = Services.ContentManager
                .Query<UserPart, UserPartRecord>()
                .Where(x => x.UserName != null)
                .List();

            var model = new UsersIndexViewModel {
                Rows = users
                    .Select(x => new UsersIndexViewModel.Row { UserPart = x })
                    .ToList()
            };

            return View(model);
        }

        public ActionResult Create() {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.New<IUser>(UserPartDriver.ContentType.Name);
            var model = new UserCreateViewModel {
                User = Services.ContentManager.BuildEditorModel(user)
            };
            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(UserCreateViewModel model) {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.New<IUser>(UserPartDriver.ContentType.Name);
            model.User = Services.ContentManager.UpdateEditorModel(user, this);
            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            string userExistsMessage = _userService.VerifyUserUnicity(model.UserName, model.Email);
            if (userExistsMessage != null) {
                AddModelError("NotUniqueUserName", T(userExistsMessage));
            }

            if (model.Password != model.ConfirmPassword) {
                AddModelError("ConfirmPassword", T("Password confirmation must match"));
            }

            user = _membershipService.CreateUser(new CreateUserParams(
                                                         model.UserName,
                                                         model.Password,
                                                         model.Email,
                                                         null, null, true));

            model.User = Services.ContentManager.UpdateEditorModel(user, this);

            if (ModelState.IsValid == false) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            return RedirectToAction("edit", new {user.Id});
        }

        public ActionResult Edit(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();
            
            return View(new UserEditViewModel {
                User = Services.ContentManager.BuildEditorModel<UserPart>(id)
            });
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();
            
            var model = new UserEditViewModel {
                User = Services.ContentManager.UpdateEditorModel<UserPart>(id, this)
            };

            TryUpdateModel(model);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            model.User.Item.NormalizedUserName = model.UserName.ToLower();

            string userExistsMessage = _userService.VerifyUserUnicity(id, model.UserName, model.Email);
            if (userExistsMessage != null) {
                AddModelError("NotUniqueUserName", T(userExistsMessage));
            }

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            Services.Notifier.Information(T("User information updated"));
            return RedirectToAction("Edit", new { id });
        }

        public ActionResult Delete(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            Services.ContentManager.Remove(Services.ContentManager.Get(id));

            Services.Notifier.Information(T("User deleted"));
            return RedirectToAction("Index");
        }

        public ActionResult Approve(int id) {
            if ( !Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")) )
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get(id);

            if ( user != null ) {
                user.As<UserPart>().RegistrationStatus = UserStatus.Approved;
                Services.Notifier.Information(T("User approved"));
            }

            return RedirectToAction("Index");
        }

        public ActionResult Moderate(int id) {
            if ( !Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")) )
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get(id);

            if ( user != null ) {
                if ( CurrentSite.SuperUser.Equals(user.As<UserPart>().UserName) ) {
                    Services.Notifier.Error(T("Super user can't be moderated"));
                }
                else {
                    user.As<UserPart>().RegistrationStatus = UserStatus.Pending;
                    Services.Notifier.Information(T("User moderated"));
                }
            }

            return RedirectToAction("Index");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }

}
