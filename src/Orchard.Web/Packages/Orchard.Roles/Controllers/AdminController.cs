using System.Web.Mvc;
using Orchard.Notify;

namespace Orchard.Roles.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly INotifier _notifier;

        public AdminController(INotifier notifier) {
            _notifier = notifier;
        }


        public ActionResult Index() {
            return View();
        }
    }
}
