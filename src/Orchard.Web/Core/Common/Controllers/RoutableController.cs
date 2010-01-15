using System.Web.Mvc;
using Orchard.Core.Common.Services;

namespace Orchard.Core.Common.Controllers {
    public class RoutableController : Controller {
        private readonly IRoutableService _routableService;

        public RoutableController(IRoutableService routableService) {
            _routableService = routableService;
        }

        [HttpPost]
        public ActionResult Slugify(FormCollection formCollection, string value) {
            return Json(_routableService.Slugify(value));
        }
    }
}