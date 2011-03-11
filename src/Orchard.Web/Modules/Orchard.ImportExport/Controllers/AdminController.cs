using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement.MetaData;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.ImportExport.Controllers {
    public class AdminController : Controller {
        private readonly IImportExportService _importExportService;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public AdminController(IOrchardServices services, IImportExportService importExportService, IContentDefinitionManager contentDefinitionManager) {
            _importExportService = importExportService;
            _contentDefinitionManager = contentDefinitionManager;
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
                _importExportService.Import(new StreamReader(Request.Files["RecipeFile"].InputStream).ReadToEnd());

                Services.Notifier.Information(T("Your recipe has been imported."));
                return RedirectToAction("Import");
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Import failed: {0}", exception.Message));
                return View();
            }
        }

        public ActionResult Export() {
            var viewModel = new ExportViewModel { ContentTypes = new List<ContentTypeEntry>() };
            foreach (var contentType in _contentDefinitionManager.ListTypeDefinitions()) {
                viewModel.ContentTypes.Add(new ContentTypeEntry { ContentTypeName = contentType.Name });
            }
            return View(viewModel);
        }

        [HttpPost, ActionName("Export")]
        public ActionResult ExportPOST() {
            if (!Services.Authorizer.Authorize(Permissions.Export, T("Not allowed to export.")))
                return new HttpUnauthorizedResult();

            var viewModel = new ExportViewModel { ContentTypes = new List<ContentTypeEntry>() };

            try {
                UpdateModel(viewModel);
                var contentTypesToExport = viewModel.ContentTypes.Where(c => c.IsChecked).Select(c => c.ContentTypeName);
                var exportFile = _importExportService.Export(contentTypesToExport, viewModel.Metadata, viewModel.Data, viewModel.SiteSettings);
                Services.Notifier.Information(T("Your export file has been created at <a href=\"{0}\" />", exportFile));

                return RedirectToAction("Export");
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Export failed: {0}", exception.Message));
                return View(viewModel);
            }
        }
    }
}