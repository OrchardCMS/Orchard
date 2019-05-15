using System.Collections.Generic;
using System.IO;
using System.Web;
using Orchard.Environment;
using Orchard.UI.Resources;
using Orchard.Utility.Extensions;
using Orchard.Workflows.Services;

namespace Orchard.Workflows {
    public class ResourceManifest : IResourceManifestProvider {
        private readonly Work<IActivitiesManager> _activitiesManager;
        private readonly IHostEnvironment _hostEnvironment;

        public ResourceManifest(Work<IActivitiesManager> activitiesManager, IHostEnvironment hostEnvironment) {
            _activitiesManager = activitiesManager;
            _hostEnvironment = hostEnvironment;
        }

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineStyle("WorkflowsAdmin").SetUrl("orchard-workflows-admin.css").SetDependencies("~/Themes/TheAdmin/Styles/Site.css");

            var resourceNames = new List<string>();

            foreach (var activity in _activitiesManager.Value.GetActivities()) {
                var assemblyName = activity.GetType().Assembly.GetName().Name;
                var basePath = VirtualPathUtility.AppendTrailingSlash("~/Modules/" + assemblyName + "/styles/");
                var resourceName = "WorkflowsActivity-" + activity.Name;
                var filename = resourceName.HtmlClassify() + ".css";

                if (File.Exists(_hostEnvironment.MapPath(basePath) + filename)) {
                    resourceNames.Add(resourceName);

                    manifest.DefineStyle(resourceName).SetUrl(filename).SetDependencies("WorkflowsAdmin");
                }
            }

            manifest.DefineStyle("WorkflowsActivities").SetUrl("workflows-activity.css").SetDependencies(resourceNames.ToArray());

            manifest.DefineScript("jsPlumb").SetUrl("jquery.jsPlumb-1.4.1-all-min.js").SetDependencies("jQueryUI");
        }
    }
}
