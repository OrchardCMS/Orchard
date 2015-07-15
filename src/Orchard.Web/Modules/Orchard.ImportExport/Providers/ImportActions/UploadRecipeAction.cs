using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ImportExport.Services;
using Orchard.UI.Notify;

namespace Orchard.ImportExport.Providers.ImportActions {
    public class UploadRecipeAction : ImportAction {
        private readonly IOrchardServices _orchardServices;
        private readonly IImportExportService _importExportService;

        public UploadRecipeAction(IOrchardServices orchardServices, IImportExportService importExportService) {
            _orchardServices = orchardServices;
            _importExportService = importExportService;
        }

        public override string Name { get { return "UploadRecipe"; } }

        public HttpPostedFileBase File { get; set; }

        public override dynamic BuildEditor(dynamic shapeFactory) {
            return UpdateEditor(shapeFactory, null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater) {
            
            if (updater != null) {
                var request = _orchardServices.WorkContext.HttpContext.Request;
                var file = request.Files["RecipeFile"];
                if (String.IsNullOrEmpty(file.FileName)) {
                    updater.AddModelError("RecipeFile", T("Please choose a recipe file to import."));
                    _orchardServices.Notifier.Error(T("Please choose a recipe file to import."));
                }
                else
                    File = file;
            }

            return shapeFactory.EditorTemplate(TemplateName: "ImportActions/UploadRecipe", Prefix: Prefix);
        }

        public override void Execute(ImportActionContext context) {
            var executionId = _importExportService.Import(new StreamReader(File.InputStream).ReadToEnd());
            context.ActionResult = new RedirectToRouteResult(new RouteValueDictionary(new { action = "ImportResult", controller = "Admin", area = "Orchard.ImportExport", executionId = executionId }));
        }
    }
}