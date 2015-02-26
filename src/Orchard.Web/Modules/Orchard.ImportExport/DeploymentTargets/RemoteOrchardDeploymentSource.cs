using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.FileSystems.AppData;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.Services;

namespace Orchard.ImportExport.DeploymentTargets {
    public class RemoteOrchardDeploymentSource : IDeploymentSource, IDeploymentSourceProvider {
        private RemoteOrchardDeploymentPart DeploymentPart { get; set; }
        private Lazy<RemoteOrchardApiClient> Client { get; set; }
        private readonly IAppDataFolder _appDataFolder;
        private readonly ISigningService _signingService;
        private readonly IClock _clock;
        private readonly UrlHelper _url;

        public RemoteOrchardDeploymentSource(IAppDataFolder appDataFolder, ISigningService signingService, IClock clock, UrlHelper url) {
            _appDataFolder = appDataFolder;
            _signingService = signingService;
            _clock = clock;
            _url = url;
        }

        public DeploymentSourceMatch Match(IContent sourceConfiguration) {
            if (!sourceConfiguration.Is<RemoteOrchardDeploymentPart>()) return null;
            
            DeploymentPart = sourceConfiguration.As<RemoteOrchardDeploymentPart>();
            Client = new Lazy<RemoteOrchardApiClient>(() => new RemoteOrchardApiClient(DeploymentPart, _signingService, _clock));
            return new DeploymentSourceMatch {DeploymentSource = this, Priority = 0};
        }

        public string GetDeploymentFile(RecipeRequest request) {
            var actionUrl = _url.Action("Recipe", "Export", new {
                area = "Orchard.ImportExport"
            });
            var data = JsonConvert.SerializeObject(request);
            var response = Client.Value.Post(actionUrl, data);
            var deploymentId = Guid.NewGuid().ToString("n");
            var filename = deploymentId
                + (response.Headers["content-type"] == "text/xml" ? ".xml" : ".nupkg");
            var path = _appDataFolder.Combine("Deployments", filename);
            using (var file = _appDataFolder.CreateFile(path)) {
                response.GetResponseStream().CopyTo(file);
            }
            return path;
        }

        public IList<DeploymentContentType> GetContentTypes() {
            var actionUrl = _url.Action("ContentTypes", "Export", new {
                area = "Orchard.ImportExport"
            });
            var result = Client.Value.Get(actionUrl);
            return JsonConvert.DeserializeObject<List<DeploymentContentType>>(result);
        }

        public IList<DeploymentQuery> GetQueries() {
            var actionUrl = _url.Action("Queries", "Export", new {
                area = "Orchard.ImportExport"
            });
            var result = Client.Value.Get(actionUrl);
            return JsonConvert.DeserializeObject<List<DeploymentQuery>>(result);
        }
    }
}
