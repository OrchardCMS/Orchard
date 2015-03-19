using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.AppData;
using Orchard.ImportExport.Permissions;
using Orchard.ImportExport.Security;
using Orchard.ImportExport.Services;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.AntiForgery;
using Orchard.Recipes.Services;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.ImportExport.Controllers {
    [OrchardFeature("Orchard.Deployment.ImportApi")]
    public class ImportController : BaseApiController {
        private readonly IImportExportService _importExportService;
        private readonly IRecipeJournal _recipeJournal;
        private readonly IAppDataFolder _appData;

        public ImportController(
            IOrchardServices services,
            IImportExportService importExportService,
            IRecipeJournal recipeJournal,
            IAppDataFolder appData,
            ISigningService signingService,
            IAuthenticationService authenticationService,
            IClock clock
            ) : base(signingService, authenticationService, clock) {

            _importExportService = importExportService;
            _recipeJournal = recipeJournal;
            _appData = appData;
            Services = services;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [AuthenticateApi]
        [HttpPost]
        [ValidateAntiForgeryTokenOrchard(false)]
        public ActionResult Recipe(string executionId) {
            if (!Services.Authorizer.Authorize(DeploymentPermissions.ImportFromDeploymentSources, T("Not allowed to import")))
                return new HttpUnauthorizedResult();

            string content;
            using (var reader = new StreamReader(Request.InputStream)) {
                content = reader.ReadToEndAsync().Result;
            }

            if (!ValidateContent(content, Request.Headers)) {
                return new HttpUnauthorizedResult(T("Invalid recipe").Text);
            }

            _importExportService.ImportRecipe(content, null, executionId);

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [AuthenticateApi]
        [HttpGet]
        public ActionResult RecipeJournal(string executionId)
        {
            if (!Services.Authorizer.Authorize(DeploymentPermissions.ImportFromDeploymentSources, T("Not allowed to import")))
                return new HttpUnauthorizedResult();

            var journal = _recipeJournal.GetRecipeJournal(executionId);

            if (!journal.Messages.Any()) {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, T("Unable to locate recipe journal").Text);
            }

            var localExecutionJournal = _recipeJournal.GetRecipeJournal(journal.Messages.First().Message);
            return CreateSignedResponse(localExecutionJournal.ExecutionId);
        }

        [AuthenticateApi]
        [HttpPost]
        [ValidateAntiForgeryTokenOrchard(false)]
        public ActionResult DeployContent(string executionId = null) {
            if (!Services.Authorizer.Authorize(DeploymentPermissions.ImportFromDeploymentSources, T("Not allowed to import")))
                return new HttpUnauthorizedResult();

            executionId = executionId ?? Guid.NewGuid().ToString("n");
            var files = Request.Files;
            if (files.Count > 0) {
                var file = files[0];
                if (!_appData.DirectoryExists("Deployments")) {
                    _appData.CreateDirectory("Deployments");
                }
                var packagePath = _appData.Combine("Deployments", executionId + ".nupkg");
                using (var packageWriteStream = _appData.CreateFile(packagePath)) {
                    file.InputStream.CopyTo(packageWriteStream);
                }
                using (var packageStream = _appData.OpenFile(packagePath)) {
                    if (!ValidateContent(packageStream, Request.Headers)) {
                        return new HttpUnauthorizedResult();
                    }
                    packageStream.Seek(0, SeekOrigin.Begin);
                    _importExportService.Import(packageStream, executionId + ".nupkg");
                }
            }
            else {
                return Recipe(executionId);
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}
