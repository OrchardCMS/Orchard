using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Handlers;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Permissions;
using Orchard.ImportExport.Security;
using Orchard.ImportExport.Services;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Projections.Models;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.ImportExport.Controllers {
    [OrchardFeature("Orchard.Deployment.ExportApi")]
    public class ExportController : BaseApiController {
        private readonly IImportExportService _importExportService;
        private readonly IDeploymentService _deploymentService;

        public ExportController(
            IOrchardServices services,
            IImportExportService importExportService,
            IDeploymentService deploymentService,
            ISigningService signingService,
            IAuthenticationService authenticationService,
            IClock clock
            ) : base(signingService, authenticationService, clock) {

            _importExportService = importExportService;
            _deploymentService = deploymentService;
            Services = services;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [AuthenticateApi]
        [HttpPost]
        public ActionResult Recipe(RecipeRequest request) {
            if (!Services.Authorizer.Authorize(DeploymentPermissions.ExportToDeploymentTargets, T("Not allowed to export")))
                return new HttpUnauthorizedResult();

            var exportingItems = _deploymentService.GetContentForExport(request);

            var unpublishStep = UnpublishedExportEventHandler.StepName +
                (request.DeployChangesAfterUtc.HasValue ? ":" + request.DeployChangesAfterUtc.Value.ToString("u") : string.Empty);

            var exportSteps = request.DeploymentMetadata != null ?
                request.DeploymentMetadata.Select(m => m.ToExportStep()).ToList() : new List<string>();
            exportSteps.Add(unpublishStep);

            var recipePath = _importExportService.Export(request.ContentTypes, exportingItems, new ExportOptions {
                ExportData = exportingItems.Any(),
                ExportMetadata = request.IncludeMetadata,
                VersionHistoryOptions = request.VersionHistoryOption,
                CustomSteps = exportSteps
            });

            var recipe = System.IO.File.ReadAllText(recipePath);
            return CreateSignedResponse(recipe);
        }

        [AuthenticateApi]
        [HttpGet]
        public ActionResult Queries() {
            if (!Services.Authorizer.Authorize(DeploymentPermissions.ExportToDeploymentTargets, T("Not allowed to export")))
                return new HttpUnauthorizedResult();

            var queries = Services.ContentManager.Query<QueryPart, QueryPartRecord>()
                .ForType(new[] {"Query"}).List()
                .Select(q =>
                    new DeploymentQuery {
                        Name = q.Name,
                        Identity = Services.ContentManager.GetItemMetadata(q.ContentItem).Identity.ToString()
                    }).ToList();

            var content = JsonConvert.SerializeObject(queries);
            return CreateSignedResponse(content);
        }

        [AuthenticateApi]
        [HttpGet]
        public ActionResult ContentTypes() {
            if (!Services.Authorizer.Authorize(DeploymentPermissions.ExportToDeploymentTargets, T("Not allowed to export")))
                return new HttpUnauthorizedResult();

            var contentTypes = Services.ContentManager.GetContentTypeDefinitions()
                .Select(c => new DeploymentContentType {Name = c.Name, DisplayName = c.DisplayName}).ToList();

            var content = JsonConvert.SerializeObject(contentTypes);
            return CreateSignedResponse(content);
        }
    }
}
