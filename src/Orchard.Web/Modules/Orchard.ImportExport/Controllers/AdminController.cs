using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ImportExport.Permissions;
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
        public ActionResult ImportPost() {
            if (!Services.Authorizer.Authorize(ImportExportPermissions.Import, T("Not allowed to import.")))
                return new HttpUnauthorizedResult();

            var recipeFile = Request.Files["RecipeFile"];
            if (recipeFile == null || String.IsNullOrEmpty(recipeFile.FileName)) {
                ModelState.AddModelError("RecipeFile", T("Please choose a recipe file or content package to import.").Text);
                Services.Notifier.Error(T("Please choose a recipe file or content package to import."));
                return View(new ImportViewModel());
            }

            if (!ModelState.IsValid) return View(new ImportViewModel());
            
            // Sets the request timeout to 10 minutes to give enough time to execute custom recipes.
            Services.WorkContext.HttpContext.Server.ScriptTimeout = 600;

            string executionId;
            try {
                executionId = _importExportService.Import(recipeFile.InputStream, recipeFile.FileName);
            }
            catch (OrchardException e) {
                Services.Notifier.Error(T("Recipe or package uploading and installation failed: {0}", e.Message));
                return View("ImportFailed");
        }
            catch (Exception e) {
                Services.Notifier.Error(T("Recipe or package uploading and installation failed: {0}", e.Message));
                return View("ImportFailed");
            }
            Services.Notifier.Information(T("Your recipe or package has been imported."));

            return RedirectToAction("ImportResult", new {ExecutionId = executionId});
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
                ExportFiles = viewModel.Files,
                ExportAsDraft = viewModel.ExportAsDraft,
                CustomSteps = customSteps
            };

            if (viewModel.Data) {
                exportOptions.ExportData = true;
                exportOptions.VersionHistoryOptions = (VersionHistoryOptions) Enum.Parse(typeof (VersionHistoryOptions), viewModel.DataImportChoice, true);
            }
            return _importExportService.Export(contentTypesToExport, exportOptions);
        }

        public ActionResult ItemRecipe(int id, string version = "Published") {
            if (!Services.Authorizer.Authorize(ImportExportPermissions.Export, T("Not allowed to export content."))) {
                return new HttpUnauthorizedResult();
            }
            if (version != "Draft" && version != "Published") return HttpNotFound();

            var versionOptions = version == "Draft" ? VersionOptions.Draft : VersionOptions.Published;
            var content = Services.ContentManager.Get(id, versionOptions);
            if (version == "Draft" && content == null) {
                content = Services.ContentManager.Get(id, VersionOptions.Latest);
            }
            if (content == null) {
                return HttpNotFound();
            }

            var exportOptions = new ExportOptions {
                ExportMetadata = false,
                ExportData = true,
                ExportSiteSettings = false,
                ExportFiles = true,
                VersionHistoryOptions = versionOptions == VersionOptions.Draft ? VersionHistoryOptions.Draft : VersionHistoryOptions.Published,
                ExportAsDraft = version == "Draft"
            };

            return _importExportService.Export(new[] {content.ContentType}, new[] {content}, exportOptions);
        }
    }
}