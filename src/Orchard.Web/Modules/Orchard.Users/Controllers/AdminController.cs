using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Users.ViewModels;
using Orchard.Mvc.Extensions;

namespace Orchard.Users.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly IMembershipService _membershipService;
        private readonly IUserService _userService;

        public AdminController(
            IOrchardServices services,
            IMembershipService membershipService,
            IUserService userService,
            IShapeFactory shapeFactory) {
            Services = services;
            _membershipService = membershipService;
            _userService = userService;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

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

            var user = Services.ContentManager.New<IUser>("User");
            var editor = Shape.EditorTemplate(TemplateName: "Parts/User.Create", Model: new UserCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            var model = Services.ContentManager.BuildEditor(user);
            model.Content.Add(editor);

            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(UserCreateViewModel createModel) {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.UserName)) {
                string userExistsMessage = _userService.VerifyUserUnicity(createModel.UserName, createModel.Email);
                if (userExistsMessage != null) {
                    AddModelError("NotUniqueUserName", T(userExistsMessage));
                }
            }

            if (createModel.Password != createModel.ConfirmPassword) {
                AddModelError("ConfirmPassword", T("Password confirmation must match"));
            }

            var user = Services.ContentManager.New<IUser>("User");
            if (ModelState.IsValid) {
                user = _membershipService.CreateUser(new CreateUserParams(
                                                  createModel.UserName,
                                                  createModel.Password,
                                                  createModel.Email,
                                                  null, null, true));
            }

            var model = Services.ContentManager.UpdateEditor(user, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/User.Create", Model: createModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                return View(model);
            }

            Services.Notifier.Information(T("User created"));
            return RedirectToAction("edit", new { user.Id });
        }

        public ActionResult Edit(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<UserPart>(id);
            var editor = Shape.EditorTemplate(TemplateName: "Parts/User.Edit", Model: new UserEditViewModel {User = user}, Prefix: null);
            editor.Metadata.Position = "2";
            var model = Services.ContentManager.BuildEditor(user);
            model.Content.Add(editor);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get(id);
            var model = Services.ContentManager.UpdateEditor(user, this);

            var editModel = new UserEditViewModel {User = user};
            TryUpdateModel(editModel);

            if (ModelState.IsValid) {
                ((IContent)model.ContentItem).As<UserPart>().NormalizedUserName = editModel.UserName.ToLower();

                string userExistsMessage = _userService.VerifyUserUnicity(id, editModel.UserName, editModel.Email);
                if (userExistsMessage != null) {
                    AddModelError("NotUniqueUserName", T(userExistsMessage));
                }
            }

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/User.Edit", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

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

        public ActionResult SendChallengeEmail(int id) {
            if ( !Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")) )
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get(id);

            if ( user != null ) {
                string challengeToken = _membershipService.GetEncryptedChallengeToken(user.As<UserPart>());
                _membershipService.SendChallengeEmail(user.As<UserPart>(), Url.AbsoluteAction(() => Url.Action("ChallengeEmail", "Account", new {Area = "Orchard.Users", token = challengeToken})));
            }

            Services.Notifier.Information(T("Challenge email sent"));

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
                if (Services.WorkContext.CurrentSite.SuperUser.Equals(user.As<UserPart>().UserName) ) {
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
