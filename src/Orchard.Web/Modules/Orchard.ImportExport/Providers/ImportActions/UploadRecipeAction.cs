using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Environment.Features;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;

namespace Orchard.ImportExport.Providers.ImportActions {
    public class UploadRecipeAction : ImportAction {
        private readonly IOrchardServices _orchardServices;
        private readonly IImportExportService _importExportService;
        private readonly ISetupService _setupService;
        private readonly ShellSettings _shellSettings;
        private readonly IFeatureManager _featureManager;

        public UploadRecipeAction(
            IOrchardServices orchardServices, 
            IImportExportService importExportService, 
            ISetupService setupService, 
            ShellSettings shellSettings, 
            IFeatureManager featureManager) {

            _orchardServices = orchardServices;
            _importExportService = importExportService;
            _setupService = setupService;
            _shellSettings = shellSettings;
            _featureManager = featureManager;
        }

        public override string Name { get { return "UploadRecipe"; } }

        public HttpPostedFileBase File { get; set; }
        public bool ResetSite { get; set; }
        public string SuperUserPassword { get; set; }

        public override dynamic BuildEditor(dynamic shapeFactory) {
            return UpdateEditor(shapeFactory, null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater) {
            var viewModel = new UploadRecipeViewModel {
                SuperUserName = _orchardServices.WorkContext.CurrentSite.SuperUser
            };

            if (updater != null) {

                if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                    var request = _orchardServices.WorkContext.HttpContext.Request;

                    File = request.Files["RecipeFile"];
                    ResetSite = viewModel.ResetSite;
                    SuperUserPassword = viewModel.SuperUserPassword;

                    if (File == null || File.ContentLength == 0)
                        updater.AddModelError("RecipeFile", T("No recipe file selected."));

                    if (ResetSite) {
                        if(String.IsNullOrWhiteSpace(viewModel.SuperUserPassword))
                            updater.AddModelError("SuperUserPassword", T("Please specify a new password for the super user."));
                        else if(!String.Equals(viewModel.SuperUserPassword, viewModel.SuperUserPasswordConfirmation))
                            updater.AddModelError("SuperUserPassword", T("The passwords do not match."));
                    }
                }
            }

            return shapeFactory.EditorTemplate(TemplateName: "ImportActions/UploadRecipe", Model: viewModel, Prefix: Prefix);
        }

        public override void Execute(ImportActionContext context) {
            if (File == null || File.ContentLength == 0)
                return;

            var executionId = ResetSite ? Setup() : ExecuteRecipe();
            context.ActionResult = new RedirectToRouteResult(new RouteValueDictionary(new { action = "ImportResult", controller = "Admin", area = "Orchard.ImportExport", executionId = executionId }));
        }

        private string Setup() {
            var setupContext = new SetupContext {
                DropExistingTables = true,
                RecipeText = ReadRecipeFile(),
                AdminPassword = SuperUserPassword,
                AdminUsername = _orchardServices.WorkContext.CurrentSite.SuperUser,
                DatabaseConnectionString = _shellSettings.DataConnectionString,
                DatabaseProvider = _shellSettings.DataProvider,
                DatabaseTablePrefix = _shellSettings.DataTablePrefix,
                SiteName = _orchardServices.WorkContext.CurrentSite.SiteName,
                EnabledFeatures = _featureManager.GetEnabledFeatures().Select(x => x.Id).ToArray()
            };
            return _setupService.Setup(setupContext);
        }

        private string ExecuteRecipe() {
            return _importExportService.Import(ReadRecipeFile());
        }

        private string ReadRecipeFile() {
            return new StreamReader(File.InputStream).ReadToEnd();
        }
    }
}