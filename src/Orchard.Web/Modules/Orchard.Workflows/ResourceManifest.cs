using System.Collections.Generic;
using System.Linq;
using Orchard.Environment;
using Orchard.UI.Resources;
using Orchard.Utility.Extensions;
using Orchard.Workflows.Services;

namespace Orchard.Workflows {
    public class ResourceManifest : IResourceManifestProvider {
        private readonly Work<IActivitiesManager> _activitiesManager;
        private readonly IHostEnvironment _hostEnviroment;

        public ResourceManifest(Work<IActivitiesManager> activitiesManager, IHostEnvironment hostEnviroment) {
            _activitiesManager = activitiesManager;
            _hostEnviroment = hostEnviroment;
        }

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineStyle("WorkflowsAdmin").SetUrl("orchard-workflows-admin.css").SetDependencies("~/Themes/TheAdmin/Styles/Site.css");

            var activities = _activitiesManager.Value.GetActivities().ToArray();

            foreach (var activity in activities) {
                var assemblyName = activity.GetType().Assembly.GetName().Name;
                var styleName = "WorkflowsActivity-" + activity.Name;
                var basePath = VirtualPathUtility.AppendTrailingSlash("~/Modules/" + assemblyName + "/styles/");
                var styleFile = styleName.HtmlClassify() + ".css";
                if (File.Exists(_hostEnviroment.MapPath(basePath) + styleFile))
                    manifest.DefineStyle(styleName)
                        .SetBasePath(basePath)
                        .SetUrl(styleFile)
                        .SetDependencies("WorkflowsAdmin");
            }

            manifest.DefineStyle("WorkflowsActivities").SetUrl("workflows-activity.css").SetDependencies(activities.Select(x => "WorkflowsActivity-" + x.Name).ToArray());

            manifest.DefineScript("jsPlumb").SetUrl("jquery.jsPlumb-1.4.1-all-min.js").SetDependencies("jQueryUI");
        }
    }
}
