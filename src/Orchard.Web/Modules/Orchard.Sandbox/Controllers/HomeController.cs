using System.Web.Mvc;

namespace Orchard.Sandbox.Controllers
{
    public class Home : Controller {
        
        public ActionResult Index()
        {            
            return RedirectToAction("index","page");
        }

    }
}
