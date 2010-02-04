using System.Web.Mvc;
using Orchard.Core.Setup.Services;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.Core.Setup.Controllers {
    public class SetupController : Controller {
        private readonly ISetupService _setupService;

        public SetupController(IOrchardServices services, ISetupService setupService) {
            _setupService = setupService;
            Services = services;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        private Localizer T { get; set; }

        public ActionResult Index() {
            return View();
        }
    }
}
