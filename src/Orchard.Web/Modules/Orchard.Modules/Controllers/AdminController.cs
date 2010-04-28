using System;
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

            var modules = _moduleService.GetInstalledModules();
            return View(new ModulesIndexViewModel {Modules = modules});
        }

        public ActionResult Edit(string moduleName) {
            if (!Services.Authorizer.Authorize(Permissions.ManageModules, T("Not allowed to edit module")))
                return new HttpUnauthorizedResult();

            var module = _moduleService.GetModuleByName(moduleName);

            if (module == null)
                return new NotFoundResult();

            return View(new ModuleEditViewModel {
                                               Name = module.DisplayName
                                           });
        }

        public ActionResult Features() {
            if (!Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to manage features")))
                return new HttpUnauthorizedResult();

            var features = _moduleService.GetAvailableFeatures();
            return View(new FeatureListViewModel {Features = features});
        }
    }
}