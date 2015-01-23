using System;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.Recipes.Models;
using Orchard.Services;

namespace Orchard.ImportExport.DeploymentTargets {
    public class OrchardDeploymentTarget : IDeploymentTarget, IDeploymentTargetProvider {
        private readonly IContentManager _contentManager;
        private readonly ISigningService _signingService;
        private RemoteOrchardDeploymentPart DeploymentPart { get; set; }
        private Lazy<RemoteOrchardApiClient> Client { get; set; }
        private readonly IClock _clock;
        private readonly UrlHelper _url;

        public OrchardDeploymentTarget(IContentManager contentManager, ISigningService signingService, IClock clock, UrlHelper url) {
            _contentManager = contentManager;
            _signingService = signingService;
            _clock = clock;
            _url = url;
        }

        public DeploymentTargetMatch Match(IContent targetConfiguration) {
            if (targetConfiguration.Is<RemoteOrchardDeploymentPart>()) {
                DeploymentPart = targetConfiguration.As<RemoteOrchardDeploymentPart>();
                Client = new Lazy<RemoteOrchardApiClient>(() => new RemoteOrchardApiClient(DeploymentPart, _signingService, _clock));
                return new DeploymentTargetMatch {DeploymentTarget = this, Priority = 0};
            }
            return null;
        }

        public void PushRecipe(string executionId, string recipeText) {
            var actionUrl = _url.Action("Recipe", "Import", new {
                area = "Orchard.ImportExport",
                executionId
            });
            Client.Value.Post(actionUrl, recipeText, "application/xml");
        }

        public RecipeStatus GetRecipeDeploymentStatus(string executionId) {
            var actionUrl = _url.Action("RecipeJournal", "Import", new {
                area = "Orchard.ImportExport",
                executionId
            });
            var journal = Client.Value.Get(actionUrl);
            var element = XElement.Parse(journal);
            var statusElement = element.Element("Status");
            RecipeStatus status;
            if (statusElement != null && Enum.TryParse(statusElement.Value, out status)) {
                return status;
            }
            return RecipeStatus.Unknown;
        }

        public void PushContent(IContent content) {
            var actionUrl = _url.Action("Content", "Import", new {
                area = "Orchard.ImportExport"
            });
            var exportedItem = _contentManager.Export(content.ContentItem);
            // TODO: handle packages
            Client.Value.Post(actionUrl, exportedItem.Data.ToString(SaveOptions.DisableFormatting));
        }
    }
}
