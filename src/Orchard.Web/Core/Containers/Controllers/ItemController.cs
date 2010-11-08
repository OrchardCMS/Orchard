using System.Web.Mvc;
using Orchard.Themes;

namespace Orchard.Core.Containers.Controllers {
    public class ItemController: Controller {
        [Themed]
        public ActionResult Display(string path, int? page) {            
            return View();
        }
    }
}