using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;
using Orchard.Localization;
using Orchard.Recipes.Services;

namespace Orchard.ImportExport.Controllers {
    public class AdminController : Controller, IUpdateModel {
        private readonly IImportExportService _importExportService;
        private readonly IRecipeResultAccessor _recipeResultAccessor;
        private readonly IEnumerable<IExportAction> _exportActions;
        private readonly IEnumerable<IImportAction> _importActions;
        private readonly IRecipeParser _recipeParser;

        public AdminController(
            IOrchardServices services, 
            IImportExportService importExportService,
            IRecipeResultAccessor recipeResultAccessor,
            IEnumerable<IExportAction> exportActions,
            IEnumerable<IImportAction> importActions,
            IRecipeParser recipeParser) {

            _importExportService = importExportService;
            _recipeResultAccessor = recipeResultAccessor;
            _exportActions = exportActions;
            _importActions = importActions;
            _recipeParser = recipeParser;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }

        public ActionResult Import() {
            var actions = _importActions.OrderByDescending(x => x.Priority).Select(x => new ImportActionViewModel {
                Editor = x.BuildEditor(Services.New)
            }).Where(x => x != null).ToList();

            var viewModel = new ImportViewModel {
                Actions = actions
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Import")]
        public ActionResult ImportPOST() {
            if (!Services.Authorizer.Authorize(Permissions.Import, T("Not allowed to import.")))
                return new HttpUnauthorizedResult();

            // Sets the request timeout to 10 minutes to give enough time to execute custom recipes.
            Services.WorkContext.HttpContext.Server.ScriptTimeout = 600;

            var actions = _importActions.OrderByDescending(x => x.Priority).ToList();
            var viewModel = new ImportViewModel {
                Actions = actions.Select(x => new ImportActionViewModel {
                    Editor = x.UpdateEditor(Services.New, this)
                }).Where(x => x != null).ToList()
            };

            if (!ModelState.IsValid) {
                return View(viewModel);
            }

            var context = new ImportActionContext { ActionResult = RedirectToAction("Import") };
            foreach(var action in actions) {
                action.Execute(context);
            }
            
            return context.ActionResult;
        }

        public ActionResult ImportResult(string executionId) {
            var result = _recipeResultAccessor.GetResult(executionId);

            var viewModel = new ImportResultViewModel() {
                Result = result
            };

            return View(viewModel);
        }

        public ActionResult Export() {
            var actions = _exportActions.OrderByDescending(x => x.Priority).Select(x => new ExportActionViewModel {
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

            var actions = _exportActions.OrderByDescending(x => x.Priority).ToList();

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