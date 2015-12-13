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

            var activityNames = _activitiesManager.Value.GetActivities().Select(x => "WorkflowsActivity-" + x.Name).ToArray();

            foreach (var activityName in activityNames) {
                manifest.DefineStyle(activityName).SetUrl(activityName.HtmlClassify()).SetDependencies("WorkflowsAdmin");
            }

            manifest.DefineStyle("WorkflowsActivities").SetUrl("workflows-activity.css").SetDependencies(activityNames);

            manifest.DefineScript("jsPlumb").SetUrl("jquery.jsPlumb-1.4.1-all-min.js").SetDependencies("jQueryUI");
        }
    }
}
