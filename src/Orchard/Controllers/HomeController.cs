using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Mvc.ModelBinders;

namespace Orchard.Controllers {
    [HandleError]
    public class HomeController : Controller {


        public ActionResult Index() {
            ViewData["Message"] = "Welcome to ASP.NET MVC!";

            return View();
        }

        public ActionResult About() {

            return View();
        }

    }

}