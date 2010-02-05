using System.Web.Mvc;
using Orchard.Core.Setup.Services;
using Orchard.Core.Setup.ViewModels;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.Core.Setup.Controllers {
    public class SetupController : Controller {

        public SetupController() {
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        private Localizer T { get; set; }

        public ActionResult Index() {
            return View(new SetupViewModel());
        }
    }
}
