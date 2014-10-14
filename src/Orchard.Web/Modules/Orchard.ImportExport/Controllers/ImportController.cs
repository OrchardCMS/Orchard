using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Permissions;
using Orchard.ImportExport.Security;
using Orchard.ImportExport.Services;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Services;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.ImportExport.Controllers {
    [OrchardFeature("Orchard.Deployment.ImportApi")]
    public class ImportController : BaseApiController {
        private readonly IImportExportService _importExportService;
        private readonly IRecipeJournal _recipeJournal;

        public ImportController(
            IOrchardServices services,
            IImportExportService importExportService,
            IRecipeJournal recipeJournal,
            ISigningService signingService,
            IAuthenticationService authenticationService,
            IClock clock
            ) : base(signingService, authenticationService, clock) {

            _importExportService = importExportService;
            _recipeJournal = recipeJournal;
            Services = services;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [AuthenticateApi]
        [HttpPost]
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

            _importExportService.Import(content);

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
        public ActionResult Content() {
            if (!Services.Authorizer.Authorize(DeploymentPermissions.ImportFromDeploymentSources, T("Not allowed to import")))
                return new HttpUnauthorizedResult();

            string content;
            using (var reader = new StreamReader(Request.InputStream)) {
                content = reader.ReadToEndAsync().Result;
            }

            if (!ValidateContent(content, Request.Headers)) {
                return new HttpUnauthorizedResult();
            }

            var contentXml = XElement.Parse(content);
            var importSession = new ImportContentSession(Services.ContentManager);
            importSession.Set(contentXml.Attribute("Id").Value, contentXml.Name.LocalName);
            Services.ContentManager.Import(contentXml, importSession);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}
