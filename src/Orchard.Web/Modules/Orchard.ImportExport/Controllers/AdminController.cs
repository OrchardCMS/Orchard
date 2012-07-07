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
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Controllers {
    public class AdminController : Controller {
        private readonly IImportExportService _importExportService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ICustomExportStep _customExportStep;

        public AdminController(
            IOrchardServices services, 
            IImportExportService importExportService, 
            IContentDefinitionManager contentDefinitionManager,
            ICustomExportStep customExportStep
            ) {
            _importExportService = importExportService;
            _contentDefinitionManager = contentDefinitionManager;
            _customExportStep = customExportStep;
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

            if (String.IsNullOrEmpty(Request.Files["RecipeFile"].FileName)) {
                ModelState.AddModelError("RecipeFile", T("Please choose a recipe file to import.").Text);
                Services.Notifier.Error(T("Please choose a recipe file to import."));
            }

            if (ModelState.IsValid) {
                _importExportService.Import(new StreamReader(Request.Files["RecipeFile"].InputStream).ReadToEnd());
                Services.Notifier.Information(T("Your recipe has been imported."));
            }

            return RedirectToAction("Import");
        }

        public ActionResult Export() {
            var customSteps = new List<string>();
            _customExportStep.Register(customSteps);

            var viewModel = new ExportViewModel {
                ContentTypes = new List<ContentTypeEntry>(),
                CustomSteps = customSteps.Select(x => new CustomStepEntry { CustomStep = x }).ToList()
            };

            foreach (var contentType in _contentDefinitionManager.ListTypeDefinitions().OrderBy(c => c.Name)) {
                viewModel.ContentTypes.Add(new ContentTypeEntry { ContentTypeName = contentType.Name });
            }

            return View(viewModel);
        }

        [HttpPost, ActionName("Export")]
        public ActionResult ExportPOST() {
            if (!Services.Authorizer.Authorize(Permissions.Export, T("Not allowed to export.")))
                return new HttpUnauthorizedResult();

            var viewModel = new ExportViewModel {
                ContentTypes = new List<ContentTypeEntry>(),
                CustomSteps = new List<CustomStepEntry>()
            };

            UpdateModel(viewModel);
            var contentTypesToExport = viewModel.ContentTypes.Where(c => c.IsChecked).Select(c => c.ContentTypeName);
            var customSteps = viewModel.CustomSteps.Where(c => c.IsChecked).Select(c => c.CustomStep);
            
            var exportOptions = new ExportOptions {
                ExportMetadata = viewModel.Metadata, 
                ExportSiteSettings = viewModel.SiteSettings,
                CustomSteps = customSteps
            };

            if (viewModel.Data) {
                exportOptions.ExportData = true;
                exportOptions.VersionHistoryOptions = (VersionHistoryOptions)Enum.Parse(typeof(VersionHistoryOptions), viewModel.DataImportChoice, true);
            }
            var exportFilePath = _importExportService.Export(contentTypesToExport, exportOptions);
            return File(exportFilePath, "text/xml", "export.xml");
        }
    }
}