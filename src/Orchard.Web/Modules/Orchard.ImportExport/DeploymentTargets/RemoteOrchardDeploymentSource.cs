using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.Services;

namespace Orchard.ImportExport.DeploymentTargets {
    public class RemoteOrchardDeploymentSource : IDeploymentSource, IDeploymentSourceProvider {
        private RemoteOrchardDeploymentPart DeploymentPart { get; set; }
        private Lazy<RemoteOrchardApiClient> Client { get; set; }
        private readonly ISigningService _signingService;
        private readonly IClock _clock;

        public RemoteOrchardDeploymentSource(ISigningService signingService, IClock clock) {
            _signingService = signingService;
            _clock = clock;
        }

        public DeploymentSourceMatch Match(IContent sourceConfiguration) {
            if (sourceConfiguration.Is<RemoteOrchardDeploymentPart>()) {
                DeploymentPart = sourceConfiguration.As<RemoteOrchardDeploymentPart>();
                Client = new Lazy<RemoteOrchardApiClient>(() => new RemoteOrchardApiClient(DeploymentPart, _signingService, _clock));
                return new DeploymentSourceMatch { DeploymentSource = this, Priority = 0 };
            }
            return null;
        }

        public string GetRecipe(RecipeRequest request) {
            const string actionUrl = "export/recipe";
            var data = JsonConvert.SerializeObject(request);
            return Client.Value.Post(actionUrl, data);
        }

        public IList<DeploymentContentType> GetContentTypes() {
            const string actionUrl = "export/contenttypes";
            var result = Client.Value.Get(actionUrl);
            return JsonConvert.DeserializeObject<List<DeploymentContentType>>(result);
        }

        public IList<DeploymentQuery> GetQueries() {
            const string actionUrl = "export/queries";
            var result = Client.Value.Get(actionUrl);
            return JsonConvert.DeserializeObject<List<DeploymentQuery>>(result);
        }
    }
}