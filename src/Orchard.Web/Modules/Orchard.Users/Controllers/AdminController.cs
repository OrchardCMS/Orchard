using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Settings.Models;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Users.ViewModels;
using Orchard.Mvc.Extensions;
using System;
using Orchard.Settings;

namespace Orchard.Users.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly IMembershipService _membershipService;
        private readonly IUserService _userService;
        private readonly ISiteService _siteService;

        public AdminController(
            IOrchardServices services,
            IMembershipService membershipService,
            IUserService userService,
            IShapeFactory shapeFactory,
            ISiteService siteService) {
            Services = services;
            _membershipService = membershipService;
            _userService = userService;
            _siteService = siteService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to list users")))
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
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.New<IUser>("User");
            var editor = Shape.EditorTemplate(TemplateName: "Parts/User.Create", Model: new UserCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(user);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(UserCreateViewModel createModel) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.UserName)) {
                if (!_userService.VerifyUserUnicity(createModel.UserName, createModel.Email)) {
                    AddModelError("NotUniqueUserName", T("User with that username and/or email already exists."));
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

            dynamic model = Services.ContentManager.UpdateEditor(user, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/User.Create", Model: createModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("User created"));
            return RedirectToAction("edit", new { user.Id });
        }

        public ActionResult Edit(int id) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<UserPart>(id);
            var editor = Shape.EditorTemplate(TemplateName: "Parts/User.Edit", Model: new UserEditViewModel {User = user}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(user);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(int id) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<UserPart>(id);
            string previousName = user.UserName;

            dynamic model = Services.ContentManager.UpdateEditor(user, this);

            var editModel = new UserEditViewModel {User = user};
            if (TryUpdateModel(editModel)) {
                if (!_userService.VerifyUserUnicity(id, editModel.UserName, editModel.Email)) {
                    AddModelError("NotUniqueUserName", T("User with that username and/or email already exists."));
                }
                else {
                    // also update the Super user if this is the renamed account
                    if (String.Equals(Services.WorkContext.CurrentSite.SuperUser, previousName, StringComparison.OrdinalIgnoreCase)) {
                        _siteService.GetSiteSettings().As<SiteSettingsPart>().SuperUser = editModel.UserName;
                    }

                    user.NormalizedUserName = editModel.UserName.ToLower();
                }
            }

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/User.Edit", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("User information updated"));
            return RedirectToAction("Edit", new { id });
        }

        public ActionResult Delete(int id) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<IUser>(id);

            if (user != null) {
                if (String.Equals(Services.WorkContext.CurrentSite.SuperUser, user.UserName, StringComparison.OrdinalIgnoreCase)) {
                    Services.Notifier.Error(T("The Super user can't be removed. Please disable this account or specify another Super user account"));
                }
                else if (String.Equals(Services.WorkContext.CurrentUser.UserName, user.UserName, StringComparison.OrdinalIgnoreCase)) {
                    Services.Notifier.Error(T("You can't remove your own account. Please log in with another account"));
                }
                else{
                    Services.ContentManager.Remove(user.ContentItem);
                    Services.Notifier.Information(T("User deleted"));
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult SendChallengeEmail(int id)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get(id);

            if ( user != null ) {
                _userService.SendChallengeEmail(user.As<UserPart>(), nonce => Url.AbsoluteAction(() => Url.Action("ChallengeEmail", "Account", new {Area = "Orchard.Users", nonce = nonce})));
            }

            Services.Notifier.Information(T("Challenge email sent"));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Approve(int id)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get(id);

            if ( user != null ) {
                user.As<UserPart>().RegistrationStatus = UserStatus.Approved;
                Services.Notifier.Information(T("User approved"));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Moderate(int id)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<IUser>(id);

            if (user != null) {
                if (String.Equals(Services.WorkContext.CurrentUser.UserName, user.UserName, StringComparison.OrdinalIgnoreCase)) {
                    Services.Notifier.Error(T("You can't disable your own account. Please log in with another account"));
                }
                else {
                    user.As<UserPart>().RegistrationStatus = UserStatus.Pending;
                    Services.Notifier.Information(T("User {0} disabled", user.UserName));
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
