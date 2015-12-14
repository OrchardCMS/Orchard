using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.Core.Settings.Models;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Users.ViewModels;
using Orchard.Mvc.Extensions;
using System;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.Utility.Extensions;

namespace Orchard.Users.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly IMembershipService _membershipService;
        private readonly IUserService _userService;
        private readonly IUserEventHandler _userEventHandlers;
        private readonly ISiteService _siteService;

        public AdminController(
            IOrchardServices services,
            IMembershipService membershipService,
            IUserService userService,
            IShapeFactory shapeFactory,
            IUserEventHandler userEventHandlers,
            ISiteService siteService) {
            Services = services;
            _membershipService = membershipService;
            _userService = userService;
            _userEventHandlers = userEventHandlers;
            _siteService = siteService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(UserIndexOptions options, PagerParameters pagerParameters) {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to list users")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new UserIndexOptions();

            var users = Services.ContentManager
                .Query<UserPart, UserPartRecord>();

            switch (options.Filter) {
                case UsersFilter.Approved:
                    users = users.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
                case UsersFilter.Pending:
                    users = users.Where(u => u.RegistrationStatus == UserStatus.Pending);
                    break;
                case UsersFilter.EmailPending:
                    users = users.Where(u => u.EmailStatus == UserStatus.Pending);
                    break;
            }

            if(!String.IsNullOrWhiteSpace(options.Search)) {
                users = users.Where(u => u.UserName.Contains(options.Search) || u.Email.Contains(options.Search));
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(users.Count());

            switch (options.Order) {
                case UsersOrder.Name:
                    users = users.OrderBy(u => u.UserName);
                    break;
                case UsersOrder.Email:
                    users = users.OrderBy(u => u.Email);
                    break;
            }

            var results = users
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new UsersIndexViewModel {
                Users = results
                    .Select(x => new UserEntry { User = x.Record })
                    .ToList(),
                    Options = options,
                    Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);

            pagerShape.RouteData(routeData);
            
            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input) {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var viewModel = new UsersIndexViewModel {Users = new List<UserEntry>(), Options = new UserIndexOptions()};
            UpdateModel(viewModel);

            var checkedEntries = viewModel.Users.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction) {
                case UsersBulkAction.None:
                    break;
                case UsersBulkAction.Approve:
                    foreach (var entry in checkedEntries) {
                        Approve(entry.User.Id);
                    }
                    break;
                case UsersBulkAction.Disable:
                    foreach (var entry in checkedEntries) {
                        Moderate(entry.User.Id);
                    }
                    break;
                case UsersBulkAction.ChallengeEmail:
                    foreach (var entry in checkedEntries) {
                        SendChallengeEmail(entry.User.Id);
                    }
                    break;
                case UsersBulkAction.Delete:
                    foreach (var entry in checkedEntries) {
                        Delete(entry.User.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
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
                if (!_userService.VerifyUserUnicity(createModel.UserName, createModel.Email)) {
                    AddModelError("NotUniqueUserName", T("User with that username and/or email already exists."));
                }
            }
            
            if (!Regex.IsMatch(createModel.Email ?? "", UserPart.EmailPattern, RegexOptions.IgnoreCase)) {
                // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
                ModelState.AddModelError("Email", T("You must specify a valid email address."));
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
            return RedirectToAction("Index");
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

            var user = Services.ContentManager.Get<UserPart>(id, VersionOptions.DraftRequired);
            string previousName = user.UserName;

            var model = Services.ContentManager.UpdateEditor(user, this);

            var editModel = new UserEditViewModel {User = user};
            if (TryUpdateModel(editModel)) {
                if (!_userService.VerifyUserUnicity(id, editModel.UserName, editModel.Email)) {
                    AddModelError("NotUniqueUserName", T("User with that username and/or email already exists."));
                }
                else if (!Regex.IsMatch(editModel.Email ?? "", UserPart.EmailPattern, RegexOptions.IgnoreCase)) {
                    // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
                    ModelState.AddModelError("Email", T("You must specify a valid email address."));
                }
                else {
                    // also update the Super user if this is the renamed account
                    if (String.Equals(Services.WorkContext.CurrentSite.SuperUser, previousName, StringComparison.Ordinal)) {
                        _siteService.GetSiteSettings().As<SiteSettingsPart>().SuperUser = editModel.UserName;
                    }

                    user.NormalizedUserName = editModel.UserName.ToLowerInvariant();
                }
            }

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/User.Edit", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                return View(model);
            }

            Services.ContentManager.Publish(user.ContentItem);

            Services.Notifier.Information(T("User information updated"));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<IUser>(id);

            if (user != null) {
                if (String.Equals(Services.WorkContext.CurrentSite.SuperUser, user.UserName, StringComparison.Ordinal)) {
                    Services.Notifier.Error(T("The Super user can't be removed. Please disable this account or specify another Super user account."));
                }
                else if (String.Equals(Services.WorkContext.CurrentUser.UserName, user.UserName, StringComparison.Ordinal)) {
                    Services.Notifier.Error(T("You can't remove your own account. Please log in with another account."));
                }
                else{
                    Services.ContentManager.Remove(user.ContentItem);
                    Services.Notifier.Information(T("User {0} deleted", user.UserName));
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult SendChallengeEmail(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<IUser>(id);

            if ( user != null ) {
                var siteUrl = Services.WorkContext.CurrentSite.BaseUrl;
                if (String.IsNullOrWhiteSpace(siteUrl)) {
                    siteUrl = HttpContext.Request.ToRootUrlString();
                }

                _userService.SendChallengeEmail(user.As<UserPart>(), nonce => Url.MakeAbsolute(Url.Action("ChallengeEmail", "Account", new { Area = "Orchard.Users", nonce = nonce }), siteUrl));
                Services.Notifier.Information(T("Challenge email sent to {0}", user.UserName));
            }


            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Approve(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<IUser>(id);

            if ( user != null ) {
                user.As<UserPart>().RegistrationStatus = UserStatus.Approved;
                Services.Notifier.Information(T("User {0} approved", user.UserName));
                _userEventHandlers.Approved(user);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Moderate(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<IUser>(id);

            if (user != null) {
                if (String.Equals(Services.WorkContext.CurrentUser.UserName, user.UserName, StringComparison.Ordinal)) {
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
