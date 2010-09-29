using System.Web.Mvc;
using Orchard.Themes;

namespace Orchard.Core.Dashboard.Controllers {
    [Themed(false)]
    public class HelperController : Controller {
        
        public ActionResult Index() {
            return View();
        }
    }
}