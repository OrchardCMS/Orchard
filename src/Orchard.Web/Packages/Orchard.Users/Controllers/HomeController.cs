using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.Users.Controllers {
    [HandleError]
    public class HomeController : Controller {
        public ActionResult Index() {
            return View();
        }
    }
}
