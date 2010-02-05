using System.Web.Mvc;
using Orchard.Setup.ViewModels;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Setup.Controllers {
    public class SetupController : Controller {
        private readonly INotifier _notifier;

        public SetupController(INotifier notifier) {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }

        public ActionResult Index() {
            return View(new SetupViewModel { AdminUsername = "admin" });
        }

        [HttpPost]
        public ActionResult Index(SetupViewModel model) {
            TryUpdateModel(model);

            if (!ModelState.IsValid) {
                return View(model);
            }
            
            // create superuser
            // set site name
            // database
            // redirect to the welcome page

            _notifier.Information(T("Setup succeeded"));
            return RedirectToAction("Index");
        }
    }
}
