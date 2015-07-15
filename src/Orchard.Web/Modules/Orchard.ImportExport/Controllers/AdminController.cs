using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;
using Orchard.Localization;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;

namespace Orchard.ImportExport.Controllers {
    public class AdminController : Controller, IUpdateModel {
        private readonly IImportExportService _importExportService;
        private readonly IRecipeResultAccessor _recipeResultAccessor;
        private readonly IEnumerable<IRecipeBuilderStep> _exportStepProviders;
        private readonly IRecipeParser _recipeParser;

        public AdminController(
            IOrchardServices services, 
            IImportExportService importExportService,
            IRecipeResultAccessor recipeResultAccessor,
            IEnumerable<IRecipeBuilderStep> exportStepProviders, 
            IRecipeParser recipeParser) {

            _importExportService = importExportService;
            _recipeResultAccessor = recipeResultAccessor;
            _exportStepProviders = exportStepProviders;
            _recipeParser = recipeParser;
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
            var exportSteps = _exportStepProviders.OrderBy(x => x.Priority).Select(x => new ExportStepViewModel {
                Name = x.Name,
                DisplayName = x.DisplayName,
                Description = x.Description,
                Editor = x.BuildEditor(Services.New)
            }).Where(x => x != null);

            var viewModel = new ExportViewModel {
                ExportSteps = exportSteps.ToList()
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Export")]
        public ActionResult ExportPOST(ExportViewModel viewModel) {
            if (!Services.Authorizer.Authorize(Permissions.Export, T("Not allowed to export.")))
                return new HttpUnauthorizedResult();
            
            var exportStepNames = viewModel.ExportSteps.Where(x => x.IsSelected).Select(x => x.Name);
            var exportStepsQuery = from name in exportStepNames
                              let provider = _exportStepProviders.SingleOrDefault(x => x.Name == name)
                              where provider != null
                              select provider;
            var exportSteps = exportStepsQuery.ToArray();
            
            foreach (var exportStep in exportSteps) {
                exportStep.UpdateEditor(Services.New, this);
            }

            var recipeDocument = _importExportService.ExportXml(exportSteps);
            var recipe = _recipeParser.ParseRecipe(recipeDocument);
            var exportFileName = GetExportFileName(recipe);
            var exportFilePath = _importExportService.WriteExportFile(recipeDocument);

            return File(exportFilePath, "text/xml", exportFileName);
        }

        private string GetExportFileName(Recipe recipe) {
            return String.IsNullOrWhiteSpace(recipe.Name) 
                ? "export.xml" 
                : String.Format(recipe.IsSetupRecipe 
                    ? "{0}.recipe.xml" 
                    : "{0}.export.xml", recipe.Name.HtmlClassify());
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}