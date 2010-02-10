using System.Web.Mvc;
using Orchard.Core.Navigation.ViewModels;
using Orchard.Localization;

namespace Orchard.Core.Navigation.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        public IOrchardServices Services { get; private set; }

        public AdminController(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            return View(new NavigationIndexViewModel());
        }
    }
}
