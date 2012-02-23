using System.Web.Mvc;
using Orchard.Themes;

namespace Orchard.Core.Common.Controllers {
    [Themed]
    public class ErrorController : Controller {

        public ActionResult NotFound(string url) {
            return HttpNotFound();
        }
    }
}