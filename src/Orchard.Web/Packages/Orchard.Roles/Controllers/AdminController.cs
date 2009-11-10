using System.Linq;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.Notify;
using Orchard.Roles.Models;
using Orchard.Roles.ViewModels;

namespace Orchard.Roles.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IRepository<RoleRecord> _roleRepository;
        private readonly INotifier _notifier;

        public AdminController(IRepository<RoleRecord> roleRepository, INotifier notifier) {
            _roleRepository = roleRepository;
            _notifier = notifier;
        }

        public ActionResult Index() {
            var model = new RolesIndexViewModel {
                                                    Rows = _roleRepository.Fetch(x => x.Name != null)
                                                        .Select(x => new RolesIndexViewModel.Row {
                                                                                                     Role = x
                                                                                                 })
                                                        .ToList()
                                                };

            return View(model);
        }

        //TODO: NYI
        public ActionResult Create() {
            var model = new RoleCreateViewModel();
            return View(model);
        }

        //TODO: NYI
        public ActionResult Edit(int id) {
            var model = new RoleEditViewModel();
            return View(model);
        }
    }
}
