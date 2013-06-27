using Orchard.UI.Resources;

namespace Orchard.Workflows {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineStyle("WorkflowsAdmin").SetUrl("orchard-workflows-admin.css").SetDependencies("~/Themes/TheAdmin/Styles/Site.css");


            builder.Add().DefineStyle("WorkflowsActivities-Branch").SetUrl("workflows-activity-branch.css").SetDependencies("WorkflowsAdmin");
            builder.Add().DefineStyle("WorkflowsActivities-ContentCreate").SetUrl("workflows-activity-contentcreated.css").SetDependencies("WorkflowsAdmin");
            builder.Add().DefineStyle("WorkflowsActivities-ContentPublished").SetUrl("workflows-activity-contentpublished.css").SetDependencies("WorkflowsAdmin");
            builder.Add().DefineStyle("WorkflowsActivities-ContentRemoved").SetUrl("workflows-activity-contentremoved.css").SetDependencies("WorkflowsAdmin");
            builder.Add().DefineStyle("WorkflowsActivities-ContentVersioned").SetUrl("workflows-activity-contentversioned.css").SetDependencies("WorkflowsAdmin");
            builder.Add().DefineStyle("WorkflowsActivities-Delete-Publish").SetUrl("workflows-activity-delete-publish.css").SetDependencies("WorkflowsAdmin");
            builder.Add().DefineStyle("WorkflowsActivities-Decision").SetUrl("workflows-activity-decision.css").SetDependencies("WorkflowsAdmin");
            builder.Add().DefineStyle("WorkflowsActivities-IsInRole").SetUrl("workflows-activity-isinrole.css").SetDependencies("WorkflowsAdmin");
            builder.Add().DefineStyle("WorkflowsActivities-Notify").SetUrl("workflows-activity-notify.css").SetDependencies("WorkflowsAdmin");
            builder.Add().DefineStyle("WorkflowsActivities-Publish").SetUrl("workflows-activity-publish.css").SetDependencies("WorkflowsAdmin");
            builder.Add().DefineStyle("WorkflowsActivities-SendEmail").SetUrl("workflows-activity-sendemail.css").SetDependencies("WorkflowsAdmin");
            builder.Add().DefineStyle("WorkflowsActivities-Timer").SetUrl("workflows-activity-timer.css").SetDependencies("WorkflowsAdmin");
            builder.Add().DefineStyle("WorkflowsActivities-UserTask").SetUrl("workflows-activity-usertask.css").SetDependencies("WorkflowsAdmin");

            builder.Add().DefineStyle("WorkflowsActivities").SetUrl("workflows-activity.css")
                .SetDependencies(
                "WorkflowsActivities-Branch",
                "WorkflowsActivities-ContentCreate",
                "WorkflowsActivities-ContentPublished",
                "WorkflowsActivities-ContentRemoved",
                "WorkflowsActivities-ContentVersioned",
                "WorkflowsActivities-Delete-Publish",
                "WorkflowsActivities-Decision",
                "WorkflowsActivities-IsInRole",
                "WorkflowsActivities-Notify",
                "WorkflowsActivities-Publish",
                "WorkflowsActivities-SendEmail",
                "WorkflowsActivities-Timer",
                "WorkflowsActivities-UserTask"
                );

            builder.Add().DefineScript("jsPlumb").SetUrl("jquery.jsPlumb-1.4.1-all-min.js").SetDependencies("jQueryUI");
        }
    }
}
