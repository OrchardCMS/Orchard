using System;
using System.Web.Mvc;
using Orchard.ImportExport.ViewModels;
using Orchard.Localization;
using Orchard.UI.Notify;

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

        [HttpPost, ActionName("Import")]
        public ActionResult ImportPOST() {
            if (!Services.Authorizer.Authorize(Permissions.Import, T("Not allowed to import.")))
                return new HttpUnauthorizedResult();

            try {
                if (String.IsNullOrEmpty(Request.Files["RecipeFile"].FileName)) {
                    throw new ArgumentException(T("Please choose a recipe file to import.").Text);
                }

                Services.Notifier.Information(T("Your recipe has been imported."));
                return RedirectToAction("Import");
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Import failed {0}", exception.Message));
                return View();
            }
        }

        public ActionResult Export() {
            var viewModel = new ExportViewModel();

            return View(viewModel);
        }
    }
}