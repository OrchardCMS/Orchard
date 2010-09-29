using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Settings;

namespace Orchard.Widgets.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        public AdminController(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;
        }

        private IOrchardServices Services { get; set; }
        protected virtual ISite CurrentSite { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            return View();
        }
    }
}
