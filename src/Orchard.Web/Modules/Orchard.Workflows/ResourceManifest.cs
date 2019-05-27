using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Caching;
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
        private readonly ICacheManager _cacheManager;

        public ResourceManifest(
            Work<IActivitiesManager> activitiesManager,
            IHostEnvironment hostEnvironment,
            IExtensionManager extensionManager,
            IVirtualPathProvider virtualPathProvider,
            ICacheManager cacheManager) {
            _activitiesManager = activitiesManager;
            _hostEnvironment = hostEnvironment;
            _extensionManager = extensionManager;
            _virtualPathProvider = virtualPathProvider;
            _cacheManager = cacheManager;
        }

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineStyle("WorkflowsAdmin").SetUrl("orchard-workflows-admin.css").SetDependencies("~/Themes/TheAdmin/Styles/Site.css");

            manifest.DefineScript("jsPlumb").SetUrl("jquery.jsPlumb-1.4.1-all-min.js").SetDependencies("jQueryUI");


            // Trying to find a matching activity CSS for each activity in the extensions they come from.
            var resourceNamesAndPaths = _cacheManager.Get("Orchard.Workflows.ActivityResourceNames", context => {
                var resourceNameAndPathList = new List<Tuple<string, string>>();

                foreach (var activity in _activitiesManager.Value.GetActivities()) {
                    var assemblyName = activity.GetType().Assembly.GetName().Name;
                    var extension = _extensionManager.GetExtension(assemblyName);
                    if (extension == null) continue;

                    var stylesPath = _virtualPathProvider.Combine(extension.VirtualPath, "Styles");
                    var resourceName = "WorkflowsActivity-" + activity.Name;
                    var filename = resourceName.HtmlClassify() + ".css";
                    var filePath = _virtualPathProvider.Combine(_hostEnvironment.MapPath(stylesPath), filename);

                    if (File.Exists(filePath)) {
                        /* Since stylesheets are shapes, we don't need to create the resource with the full path to the CSS file,
                         * because extensions can override those shapes by file name if they reference Orchard.Workflows,
                         * even when they don't exist in Orchard.Workflows. */
                        resourceNameAndPathList.Add(Tuple.Create(resourceName, filename));
                    }
                }

                return resourceNameAndPathList;
            });

            foreach (var resourceNameAndPath in resourceNamesAndPaths) {
                manifest
                    .DefineStyle(resourceNameAndPath.Item1)
                    .SetUrl(resourceNameAndPath.Item2)
                    .SetDependencies("WorkflowsAdmin");
            }

            manifest
                .DefineStyle("WorkflowsActivities")
                .SetDependencies(resourceNamesAndPaths.Select(resourceNameAndPath => resourceNameAndPath.Item1).ToArray());
        }
    }
}
