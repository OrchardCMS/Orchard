using System.Linq;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.Models;
using Orchard.Notify;
using Orchard.Users.Models;
using Orchard.Users.ViewModels;

namespace Orchard.Users.Controllers {
    public class AdminController : Controller {
        private readonly IModelManager _modelManager;
        private readonly IRepository<UserRecord> _userRepository;
        private readonly INotifier _notifier;

        public AdminController(
            IModelManager modelManager,
            IRepository<UserRecord> userRepository,
            INotifier notifier) {
            _modelManager = modelManager;
            _userRepository = userRepository;
            _notifier = notifier;
        }

        public ActionResult Index() {
            var model = new UsersIndexViewModel();
            model.Rows = _userRepository.Fetch(x => x.UserName != null)
                .Select(x => new UsersIndexViewModel.Row {
                    User = _modelManager.Get(x.Id).As<UserModel>()
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
            if (ModelState.IsValid == false) {
                return View(model);
            }
            var user = _modelManager.New("user");
            user.As<UserModel>().Record = new UserRecord { UserName = model.UserName, Email = model.Email };
            _modelManager.Create(user);
            return RedirectToAction("edit", new { user.Id });
        }

        public ActionResult Edit(int id) {
            var model = new UserEditViewModel { User = _modelManager.Get(id) };
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(int id, FormCollection input) {
            var model = new UserEditViewModel { User = _modelManager.Get(id) };
            if (!TryUpdateModel(model, input.ToValueProvider())) {
                return View(model);
            }
            _notifier.Information("User information updated");
            return RedirectToAction("Edit", new { id });
        }
    }

}
