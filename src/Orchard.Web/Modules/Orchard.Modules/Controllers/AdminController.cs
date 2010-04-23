using System.Web.Mvc;
using Orchard.Modules.ViewModels;

namespace Orchard.Modules.Controllers {
    public class AdminController : Controller {
        private readonly IModuleService _moduleService;

        public AdminController(IModuleService moduleService) {
            _moduleService = moduleService;
        }

        public ActionResult Index() {
            var modules = _moduleService.GetInstalledModules();
            return View(new ModulesIndexViewModel {Modules = modules});
        }
    }
}