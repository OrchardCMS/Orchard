using System.Web.Mvc;
using Orchard.Mvc.ViewModels;

namespace Orchard.Controllers {
    [HandleError]
    public class HomeController : Controller {
        public ActionResult Index() {
            ViewData["Message"] = "Welcome to ASP.NET MVC!";

            return View(new BaseViewModel());
        }

        public ActionResult About() {

            return View();
        }
    }
}