using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ImportExport.Services;

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
                File = request.Files["RecipeFile"];
            }

            return shapeFactory.EditorTemplate(TemplateName: "ImportActions/UploadRecipe", Prefix: Prefix);
        }

        public override void Execute(ImportActionContext context) {
            if (File == null || File.ContentLength == 0)
                return;

            var executionId = _importExportService.Import(new StreamReader(File.InputStream).ReadToEnd());
            context.ActionResult = new RedirectToRouteResult(new RouteValueDictionary(new { action = "ImportResult", controller = "Admin", area = "Orchard.ImportExport", executionId = executionId }));
        }
    }
}