using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement.MetaData;
using Orchard.ImportExport.Permissions;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;
using Orchard.Localization;
using Orchard.Recipes.Services;
using Orchard.UI.Notify;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Controllers {
    public class AdminController : Controller {
        private readonly IImportExportService _importExportService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ICustomExportStep _customExportStep;
        private readonly IRecipeJournal _recipeJournal;

        public AdminController(
            IOrchardServices services,
            IImportExportService importExportService,
            IContentDefinitionManager contentDefinitionManager,
            ICustomExportStep customExportStep,
            IRecipeJournal recipeJournal
            ) {
            _importExportService = importExportService;
            _contentDefinitionManager = contentDefinitionManager;
            _customExportStep = customExportStep;
            _recipeJournal = recipeJournal;
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
        public ActionResult ImportPost() {
            if (!Services.Authorizer.Authorize(ImportExportPermissions.Import, T("Not allowed to import.")))
                return new HttpUnauthorizedResult();

            var recipeFile = Request.Files["RecipeFile"];
            if (recipeFile == null || String.IsNullOrEmpty(recipeFile.FileName)) {
                ModelState.AddModelError("RecipeFile", T("Please choose a recipe file to import.").Text);
                Services.Notifier.Error(T("Please choose a recipe file to import."));
                return View(new ImportViewModel());
            }

            if (!ModelState.IsValid) return View(new ImportViewModel());

            var executionId = _importExportService.Import(new StreamReader(recipeFile.InputStream).ReadToEnd());
            Services.Notifier.Information(T("Your recipe has been imported."));

            return RedirectToAction("ImportResult", new {ExecutionId = executionId});
        }

        public ActionResult ImportResult(string executionId) {
            var journal = _recipeJournal.GetRecipeJournal(executionId);
            return View(journal);
        }

        public ActionResult Export() {
            var customSteps = new List<string>();
            _customExportStep.Register(customSteps);

            var viewModel = new ExportViewModel {
                ContentTypes = new List<ContentTypeEntry>(),
                CustomSteps = customSteps.Select(x => new CustomStepEntry {CustomStep = x}).ToList()
            };

            foreach (var contentType in _contentDefinitionManager.ListTypeDefinitions().OrderBy(c => c.Name)) {
                viewModel.ContentTypes.Add(new ContentTypeEntry {ContentTypeName = contentType.Name});
            }

            return View(viewModel);
        }

        [HttpPost, ActionName("Export")]
        public ActionResult ExportPost() {
            if (!Services.Authorizer.Authorize(ImportExportPermissions.Export, T("Not allowed to export."))) {
                return new HttpUnauthorizedResult();
            }

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
                exportOptions.VersionHistoryOptions = (VersionHistoryOptions) Enum.Parse(typeof (VersionHistoryOptions), viewModel.DataImportChoice, true);
            }
            return _importExportService.Export(contentTypesToExport, exportOptions);
        }
    }
}