using System.Collections.Generic;
using System.Linq;
using Orchard.Environment;
using Orchard.UI.Resources;
using Orchard.Utility.Extensions;
using Orchard.Workflows.Services;

namespace Orchard.Workflows {
    public class ResourceManifest : IResourceManifestProvider {
        private readonly Work<IActivitiesManager> _activitiesManager;

        public ResourceManifest(Work<IActivitiesManager> activitiesManager) {
            _activitiesManager = activitiesManager;
        }

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineStyle("WorkflowsAdmin").SetUrl("orchard-workflows-admin.css").SetDependencies("~/Themes/TheAdmin/Styles/Site.css");

            var activities = _activitiesManager.Value.GetActivities().ToArray();//.Select(x => "WorkflowsActivity-" + x.Name).ToArray();

            foreach (var activity in activities) {
                var assemblyName = activity.GetType().Assembly.GetName().Name;
                var styleName = "WorkflowsActivity-" + activity.Name;
                manifest.DefineStyle(styleName)
                    .SetBasePath(VirtualPathUtility.AppendTrailingSlash("~/Modules/"+assemblyName+"/styles/"))
                    .SetUrl(styleName.HtmlClassify()+".css")
                    .SetDependencies("WorkflowsAdmin");
            }

            manifest.DefineStyle("WorkflowsActivities").SetUrl("workflows-activity.css").SetDependencies(activities.Select(x => "WorkflowsActivity-" + x.Name).ToArray());

            manifest.DefineScript("jsPlumb").SetUrl("jquery.jsPlumb-1.4.1-all-min.js").SetDependencies("jQueryUI");
        }
    }
}
