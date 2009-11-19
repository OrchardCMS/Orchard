using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using Orchard.Users.ViewModels;

namespace Orchard.Users.Controllers {

    public class AdminController : Controller, IModelUpdater {
        private readonly IMembershipService _membershipService;
        private readonly IContentManager _contentManager;
        private readonly IRepository<UserRecord> _userRepository;
        private readonly INotifier _notifier;

        public AdminController(
            IMembershipService membershipService,
            IContentManager contentManager,
            IRepository<UserRecord> userRepository,
            INotifier notifier) {
            _membershipService = membershipService;
            _contentManager = contentManager;
            _userRepository = userRepository;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        public IUser CurrentUser { get; set; }

        public Localizer T { get; set; }

        public ActionResult Index() {
            var model = new UsersIndexViewModel();
            model.Rows = _userRepository.Fetch(x => x.UserName != null)
                .Select(x => new UsersIndexViewModel.Row {
                    User = _contentManager.Get(x.Id).As<UserModel>()
                })
                .ToList();

            return View(model);
        }

        public ActionResult Create() {
            var model = new UserCreateViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(UserCreateViewModel model) {
            if (model.Password != model.ConfirmPassword) {
                ModelState.AddModelError("ConfirmPassword", T("Password confirmation must match").ToString());
            }
            if (ModelState.IsValid == false) {
                return View(model);
            }
            var user = _membershipService.CreateUser(new CreateUserParams(
                                              model.UserName,
                                              model.Password,
                                              model.Email,
                                              null, null, true));
            return RedirectToAction("edit", new { user.Id });
        }

        public ActionResult Edit(int id) {
            var model = new UserEditViewModel { User = _contentManager.Get<UserModel>(id) };
            model.Editors = _contentManager.GetEditors(model.User.ContentItem);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(int id, FormCollection input) {
            var model = new UserEditViewModel { User = _contentManager.Get<UserModel>(id) };
            model.Editors = _contentManager.UpdateEditors(model.User.ContentItem, this);

            if (!TryUpdateModel(model, input.ToValueProvider())) {
                return View(model);
            }

            _notifier.Information(T("User information updated"));
            return RedirectToAction("Edit", new { id });
        }


        bool IModelUpdater.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
    }

}
