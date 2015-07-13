using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement.MetaData;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;
using Orchard.Localization;
using Orchard.Recipes.Services;
using Orchard.UI.Notify;
using Orchard.ImportExport.Models;
using Orchard.Utility.Extensions;

namespace Orchard.ImportExport.Controllers {
    public class AdminController : Controller {
        private readonly IImportExportService _importExportService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ICustomExportStep _customExportStep;
        private readonly IRecipeResultAccessor _recipeResultAccessor;

        public AdminController(
            IOrchardServices services, 
            IImportExportService importExportService, 
            IContentDefinitionManager contentDefinitionManager,
            ICustomExportStep customExportStep,
            IRecipeResultAccessor recipeResultAccessor) {
            _importExportService = importExportService;
            _contentDefinitionManager = contentDefinitionManager;
            _customExportStep = customExportStep;
            _recipeResultAccessor = recipeResultAccessor;
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
                var executionId = _importExportService.Import(new StreamReader(Request.Files["RecipeFile"].InputStream).ReadToEnd());
                return RedirectToAction("ImportResult", new { executionId = executionId });
            }

            return View(new ImportViewModel());
        }

        public ActionResult ImportResult(string executionId) {
            var result = _recipeResultAccessor.GetResult(executionId);

            var viewModel = new ImportResultViewModel() {
                Result = result
            };

            return View(viewModel);
        }

        public ActionResult Export() {
            var customSteps = new List<string>();
            _customExportStep.Register(customSteps);

            var viewModel = new ExportViewModel {
                RecipeVersion = "1.0",
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
            var contentTypesToExport = viewModel.ContentTypes.Where(c => c.IsChecked).Select(c => c.ContentTypeName).ToList();
            var customSteps = viewModel.CustomSteps.Where(c => c.IsChecked).Select(c => c.CustomStep);
            
            var exportOptions = new ExportOptions {
                ExportMetadata = viewModel.Metadata, 
                ExportSiteSettings = viewModel.SiteSettings,
                SetupRecipe = viewModel.SetupRecipe,
                RecipeName = viewModel.RecipeName,
                RecipeDescription = viewModel.RecipeDescription,
                RecipeWebsite = viewModel.RecipeWebsite,
                RecipeTags = viewModel.RecipeTags,
                RecipeVersion = viewModel.RecipeVersion,
                CustomSteps = customSteps
            };

            if (viewModel.Data) {
                exportOptions.ExportData = true;
                exportOptions.VersionHistoryOptions = (VersionHistoryOptions)Enum.Parse(typeof(VersionHistoryOptions), viewModel.DataImportChoice, true);
                exportOptions.ImportBatchSize = viewModel.ImportBatchSize;
            }
            var exportFilePath = _importExportService.Export(contentTypesToExport, exportOptions);
            var exportFileName = exportOptions.SetupRecipe && !String.IsNullOrWhiteSpace(exportOptions.RecipeName)
                ? String.Format("{0}.recipe.xml", exportOptions.RecipeName.HtmlClassify())
                : "export.xml";

            return File(exportFilePath, "text/xml", exportFileName);
        }
    }
}