using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;
using Orchard.Localization;
using Orchard.Recipes.Services;
using Orchard.UI.Notify;

namespace Orchard.ImportExport.Controllers {
    public class AdminController : Controller, IUpdateModel {
        private readonly IImportExportService _importExportService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ICustomExportStep _customExportStep;
        private readonly IRecipeResultAccessor _recipeResultAccessor;
        private readonly IEnumerable<IExportStepProvider> _exportStepProviders;

        public AdminController(
            IOrchardServices services, 
            IImportExportService importExportService, 
            IContentDefinitionManager contentDefinitionManager,
            ICustomExportStep customExportStep,
            IRecipeResultAccessor recipeResultAccessor, 
            IEnumerable<IExportStepProvider> exportStepProviders) {

            _importExportService = importExportService;
            _contentDefinitionManager = contentDefinitionManager;
            _customExportStep = customExportStep;
            _recipeResultAccessor = recipeResultAccessor;
            _exportStepProviders = exportStepProviders;
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

            var exportSteps = _exportStepProviders.OrderBy(x => x.Position).Select(x => new ExportStepViewModel {
                Name = x.Name,
                DisplayName = x.DisplayName,
                Description = x.Description,
                Editor = x.BuildEditor(Services.New)
            }).Where(x => x != null);

            var viewModel = new ExportViewModel {
                CustomSteps = customSteps.Select(x => new CustomStepEntry { CustomStep = x }).ToList(),
                ExportSteps = exportSteps.ToList()
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Export")]
        public ActionResult ExportPOST() {
            if (!Services.Authorizer.Authorize(Permissions.Export, T("Not allowed to export.")))
                return new HttpUnauthorizedResult();

            var viewModel = new ExportViewModel {
                CustomSteps = new List<CustomStepEntry>(),
                ExportSteps = new List<ExportStepViewModel>()
            };

            UpdateModel(viewModel);
            var exportStepNames = viewModel.ExportSteps.Where(x => x.IsSelected).Select(x => x.Name);
            var exportStepsQuery = from name in exportStepNames
                              let provider = _exportStepProviders.SingleOrDefault(x => x.Name == name)
                              where provider != null
                              select provider;
            var exportSteps = exportStepsQuery.ToArray();
            var customSteps = viewModel.CustomSteps.Where(c => c.IsChecked).Select(c => c.CustomStep);
            
            var exportOptions = new ExportOptions {
                CustomSteps = customSteps
            };

            foreach (var exportStep in exportSteps) {
                exportStep.UpdateEditor(Services.New, this);
            }

            var exportFilePath = _importExportService.Export(exportSteps, exportOptions);
            var exportFileName = "export.xml";

            return File(exportFilePath, "text/xml", exportFileName);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}