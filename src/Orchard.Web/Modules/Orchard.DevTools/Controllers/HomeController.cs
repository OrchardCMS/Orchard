using System.Web.Mvc;
using Orchard.DevTools.Models;
using Orchard.Mvc.ViewModels;
using Orchard.UI.Notify;

namespace Orchard.DevTools.Controllers {
    //[Themed]
    public class HomeController : Controller {
        private readonly INotifier _notifier;

        public HomeController(INotifier notifier) {
            _notifier = notifier;
        }

        public ActionResult Index() {
            return View(new BaseViewModel());
        }

        public ActionResult NotAuthorized() {
            _notifier.Warning("Simulated error goes here.");
            return new HttpUnauthorizedResult();
        }

        public ActionResult Simple() {
            return View(new Simple { Title = "This is a simple text", Quantity = 5 });
        }

        public ActionResult _RenderableAction() {
            return PartialView("_RenderableAction", "This is render action");
        }
    }
}
