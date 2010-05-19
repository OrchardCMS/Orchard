using System.Linq;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Modules.ViewModels;
using Orchard.Mvc.Results;

namespace Orchard.Modules.Controllers {
    public class AdminController : Controller {
        private readonly IModuleService _moduleService;

        public AdminController(IOrchardServices services, IModuleService moduleService) {
            Services = services;
            _moduleService = moduleService;
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public ActionResult Index() {
            if (!Services.Authorizer.Authorize(Permissions.ManageModules, T("Not allowed to manage modules")))
                return new HttpUnauthorizedResult();

            var modules = _moduleService.GetInstalledModules().ToList();
            return View(new ModulesIndexViewModel {Modules = modules});
        }

        public ActionResult Features() {
            if (!Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to manage features")))
                return new HttpUnauthorizedResult();

            var features = _moduleService.GetAvailableFeatures().ToList();
            return View(new FeaturesViewModel {Features = features});
        }

        [HttpPost]
        public ActionResult Enable(string id, bool? force) {
            if (!Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to manage features")))
                return new HttpUnauthorizedResult();

            if (string.IsNullOrEmpty(id))
                return new NotFoundResult();

            _moduleService.EnableFeatures(new[] {id}, force != null && (bool) force);

            return RedirectToAction("Features");
        }

        [HttpPost]
        public ActionResult Disable(string id, bool? force) {
            if (!Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to manage features")))
                return new HttpUnauthorizedResult();

            if (string.IsNullOrEmpty(id))
                return new NotFoundResult();

            _moduleService.DisableFeatures(new[] {id}, force != null && (bool) force);

            return RedirectToAction("Features");
        }
    }
}