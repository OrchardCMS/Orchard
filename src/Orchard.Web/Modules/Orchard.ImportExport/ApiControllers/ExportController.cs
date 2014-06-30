using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;
using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
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

namespace Orchard.ImportExport.ApiControllers {
    [OrchardFeature("Orchard.Deployment.ExportApi")]
    public class ExportController : BaseApiController {
        private readonly IImportExportService _importExportService;
        private readonly IDeploymentService _deploymentService;

        public ExportController(IOrchardServices services,
            IImportExportService importExportService,
            IDeploymentService deploymentService,
            ISigningService signingService,
            IAuthenticationService authenticationService,
            IClock clock,
            IShapeFactory shapeFactory) : base(signingService,authenticationService,clock) {
            _importExportService = importExportService;
            _deploymentService = deploymentService;
            Services = services;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Shape = shapeFactory;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        dynamic Shape { get; set; }

        [AuthenticateApi]
        [HttpPost]
        public HttpResponseMessage Recipe(RecipeRequest request) {
            if (!Services.Authorizer.Authorize(DeploymentPermissions.ExportToDeploymentTargets, T("Not allowed to export")))
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var exportingItems = _deploymentService.GetContentForExport(request);

            var unpublishStep = UnpublishedExportEventHandler.StepName +
                (request.DeployChangesAfterUtc.HasValue ? ":" + request.DeployChangesAfterUtc.Value.ToString("u") : string.Empty);

            var exportSteps = request.DeploymentMetadata != null ?
                request.DeploymentMetadata.Select(m => m.ToExportStep()).ToList() : new List<string>();
            exportSteps.Add(unpublishStep);

            var recipePath = _importExportService.Export(request.ContentTypes, exportingItems, new ExportOptions
            {
                ExportData = exportingItems.Any(),
                ExportMetadata = request.IncludeMetadata,
                VersionHistoryOptions = request.VersionHistoryOption,
                CustomSteps = exportSteps
            });

            var recipe = File.ReadAllText(recipePath);
            return CreateSignedResponse(recipe);
        }

        [AuthenticateApi]
        [HttpGet]
        public HttpResponseMessage Queries() {
            if (!Services.Authorizer.Authorize(DeploymentPermissions.ExportToDeploymentTargets, T("Not allowed to export")))
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var queries = Services.ContentManager.Query<QueryPart, QueryPartRecord>()
                                  .ForType(new[] { "Query" }).List()
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
        public HttpResponseMessage ContentTypes() {
            if (!Services.Authorizer.Authorize(DeploymentPermissions.ExportToDeploymentTargets, T("Not allowed to export")))
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var contentTypes = Services.ContentManager.GetContentTypeDefinitions()
                .Select(c => new DeploymentContentType { Name = c.Name, DisplayName = c.DisplayName }).ToList();

            var content = JsonConvert.SerializeObject(contentTypes);
            return CreateSignedResponse(content);
        }
    }
}
