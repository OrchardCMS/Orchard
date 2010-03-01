using System.Web.Mvc;
using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Dashboard.Controllers {
    public class AdminController : Controller {
        public ActionResult Index() {
            return View(new AdminViewModel());
        }
    }
}