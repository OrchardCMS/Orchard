using System.Web.Mvc;
using Orchard.Modules.ViewModels;

namespace Orchard.Modules.Controllers {
    public class AdminController : Controller {
        public ActionResult Index() {
            return View(new ModulesIndexViewModel());
        }
    }
}