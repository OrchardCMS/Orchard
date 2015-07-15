using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;
using Orchard.Localization;
using Orchard.Recipes.Services;
using Orchard.UI.Notify;

namespace Orchard.ImportExport.Controllers {
    public class AdminController : Controller, IUpdateModel {
        private readonly IImportExportService _importExportService;
        private readonly IRecipeResultAccessor _recipeResultAccessor;
        private readonly IEnumerable<IExportAction> _exportActions;
        private readonly IRecipeParser _recipeParser;

        public AdminController(
            IOrchardServices services, 
            IImportExportService importExportService,
            IRecipeResultAccessor recipeResultAccessor,
            IEnumerable<IExportAction> exportActions, 
            IRecipeParser recipeParser) {

            _importExportService = importExportService;
            _recipeResultAccessor = recipeResultAccessor;
            _exportActions = exportActions;
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
            var actions = _exportActions.OrderBy(x => x.Priority).Select(x => new ExportActionViewModel {
                Editor = x.BuildEditor(Services.New)
            }).Where(x => x != null).ToList();

            var viewModel = new ExportViewModel {
                Actions = actions
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Export")]
        public ActionResult ExportPOST(ExportViewModel viewModel) {
            if (!Services.Authorizer.Authorize(Permissions.Export, T("Not allowed to export.")))
                return new HttpUnauthorizedResult();

            var actions = _exportActions.OrderBy(x => x.Priority).ToList();

            foreach (var action in actions) {
                action.UpdateEditor(Services.New, this);
            }

            var exportActionContext = new ExportActionContext {
                ActionResult = RedirectToAction("Export")
            };

            foreach (var action in actions) {
                action.Execute(exportActionContext);
            }
            
            return exportActionContext.ActionResult;
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}