using System.Collections.Generic;
using System.IO;
using System.Web;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.VirtualPath;
using Orchard.UI.Resources;
using Orchard.Utility.Extensions;
using Orchard.Workflows.Services;

namespace Orchard.Workflows {
    public class ResourceManifest : IResourceManifestProvider {
        private readonly Work<IActivitiesManager> _activitiesManager;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IExtensionManager _extensionManager;
        private readonly IVirtualPathProvider _virtualPathProvider;

        public ResourceManifest(
            Work<IActivitiesManager> activitiesManager,
            IHostEnvironment hostEnvironment,
            IExtensionManager extensionManager,
            IVirtualPathProvider virtualPathProvider) {
            _activitiesManager = activitiesManager;
            _hostEnvironment = hostEnvironment;
            _extensionManager = extensionManager;
            _virtualPathProvider = virtualPathProvider;
        }

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineStyle("WorkflowsAdmin").SetUrl("orchard-workflows-admin.css").SetDependencies("~/Themes/TheAdmin/Styles/Site.css");

            var resourceNames = new List<string>();

            foreach (var activity in _activitiesManager.Value.GetActivities()) {
                var assemblyName = activity.GetType().Assembly.GetName().Name;
                var descriptor = _extensionManager.GetExtension(assemblyName);
                if (descriptor == null) continue;

                var stylesPath = _virtualPathProvider.Combine(descriptor.VirtualPath, "Styles");
                var resourceName = "WorkflowsActivity-" + activity.Name;
                var filename = resourceName.HtmlClassify() + ".css";
                var filePath = _virtualPathProvider.Combine(_hostEnvironment.MapPath(stylesPath), filename);

                if (File.Exists(filePath)) {
                    resourceNames.Add(resourceName);

                    manifest.DefineStyle(resourceName).SetUrl(filename).SetDependencies("WorkflowsAdmin");
                }
            }

            manifest.DefineStyle("WorkflowsActivities").SetUrl("workflows-activity.css").SetDependencies(resourceNames.ToArray());

            manifest.DefineScript("jsPlumb").SetUrl("jquery.jsPlumb-1.4.1-all-min.js").SetDependencies("jQueryUI");
        }
    }
}
