using System.Web.Mvc;
using Orchard.ImportExport.ViewModels;
using Orchard.Localization;

namespace Orchard.ImportExport.Controllers {
    public class AdminController : Controller {
        public AdminController(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }

        public ActionResult Import() {
            var viewModel = new ImportViewModel();

            return View(viewModel);
        }

        public ActionResult Export() {
            var viewModel = new ExportViewModel();

            return View(viewModel);
        }
    }
}