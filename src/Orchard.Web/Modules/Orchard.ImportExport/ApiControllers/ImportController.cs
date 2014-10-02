using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
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

namespace Orchard.ImportExport.ApiControllers {
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
        [AcceptVerbs("POST")]
        public HttpResponseMessage Recipe(string executionId) {
            if (!Services.Authorizer.Authorize(DeploymentPermissions.ImportFromDeploymentSources, T("Not allowed to import")))
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var content = Request.Content.ReadAsStringAsync().Result;

            if (!ValidateContent(content, Request.Headers)) {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            _importExportService.Import(content);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [AuthenticateApi]
        [AcceptVerbs("GET")]
        public HttpResponseMessage RecipeJournal(string executionId)
        {
            if (!Services.Authorizer.Authorize(DeploymentPermissions.ImportFromDeploymentSources, T("Not allowed to import")))
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var journal = _recipeJournal.GetRecipeJournal(executionId);
            if (journal.Messages.Any()) {
                var localExecutionJournal = _recipeJournal.GetRecipeJournal(journal.Messages.First().Message);
                return CreateSignedResponse(localExecutionJournal.ExecutionId);
            }

            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate recipe journal");
        }

        [AuthenticateApi]
        [AcceptVerbs("POST")]
        public HttpResponseMessage Content() {
            if (!Services.Authorizer.Authorize(DeploymentPermissions.ImportFromDeploymentSources, T("Not allowed to import")))
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var content = Request.Content.ReadAsStringAsync().Result;

            if (!ValidateContent(content, Request.Headers)) {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var contentXml = XElement.Parse(content);
            var importSession = new ImportContentSession(Services.ContentManager);
            importSession.Set(contentXml.Attribute("Id").Value, contentXml.Name.LocalName);
            Services.ContentManager.Import(contentXml, importSession);
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
