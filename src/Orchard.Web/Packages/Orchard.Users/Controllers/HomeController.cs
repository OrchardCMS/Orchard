using System.Web.Mvc;

namespace Orchard.Users.Controllers {
    [HandleError]
    public class HomeController : Controller {
        public ActionResult Index() {
            return View();
        }
    }
}
