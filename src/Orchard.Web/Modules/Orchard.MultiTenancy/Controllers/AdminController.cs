using System.Web.Mvc;
using Orchard.Localization;
using Orchard.MultiTenancy.ViewModels;

namespace Orchard.MultiTenancy.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        public AdminController() {
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }

        public ActionResult List() {
            return View(new TenantsListViewModel());
        }
    }
}