using System.Web.Mvc;
using Orchard.Mvc.ViewModels;
using Orchard.UI.Notify;

namespace Orchard.DevTools.Controllers
{
    public class HomeController : Controller
    {
        private readonly INotifier _notifier;

        public HomeController(INotifier notifier) {
            _notifier = notifier;
        }

        public ActionResult Index()
        {
            return View(new BaseViewModel());
        }

        public ActionResult NotAuthorized() {
            _notifier.Warning("Simulated error goes here.");
            return new HttpUnauthorizedResult();
        }

    }
}
