using System;
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

        public OrchardDeploymentTarget(IContentManager contentManager, ISigningService signingService, IClock clock) {
            _contentManager = contentManager;
            _signingService = signingService;
            _clock = clock;
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
            var actionUrl = string.Format("import/recipe?executionId={0}", executionId);
            Client.Value.Post(actionUrl, recipeText);
        }

        public RecipeStatus GetRecipeDeploymentStatus(string executionId) {
            var actionUrl = string.Format("import/recipejournal?executionId={0}", executionId);
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
            const string actionUrl = "import/content";
            var exportedItem = _contentManager.Export(content.ContentItem);
            Client.Value.Post(actionUrl, exportedItem.ToString());
        }
    }
}
