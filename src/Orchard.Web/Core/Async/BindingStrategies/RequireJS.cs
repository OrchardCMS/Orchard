using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Extensions;
using Orchard.UI.Resources;

namespace Orchard.Core.Async.BindingStrategies {
    [OrchardFeature("Async.RequireJS")]
    public class RequireJS : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("AsyncScriptsResourceWriter").OnDisplaying(context => {
                context.ShapeMetadata.Type = "RequireJSAsyncScriptsWriter";
            });
        }

        [Shape]
        public void RequireJSAsyncScriptsWriter(dynamic Shape, dynamic Display, TextWriter Output) {
            var defaultSettings = (RequireSettings) Shape.DefaultSettings;
            var appPath = (string) Shape.AppPath;
            var resources = (IEnumerable<ResourceRequiredContext>) Shape.Resources;

            if (!resources.Any())
                return;

            var dictionary = resources.ToDictionary(x => x.Resource.Name);
            var resourcesWithDependencies = resources.Where(x => x.Resource.Dependencies != null && x.Resource.Dependencies.Any()).ToArray();
            var scriptBuilder = new StringBuilder();

            // Configure dependencies
            if (resourcesWithDependencies.Any()) {
                scriptBuilder.AppendFormat(
                    "requirejs.config({{" +
                    "shim: {{\r\n");

                var index = 0;
                foreach (var resource in resourcesWithDependencies) {
                    var resourceName = GetModuleName(defaultSettings, appPath, resource);
                    var dependencies = String.Join(",", resource.Resource.Dependencies.Select(x => String.Format("\"{0}\"", GetModuleName(defaultSettings, appPath, dictionary[x]))));
                    scriptBuilder.AppendFormat("\"{0}\": [{1}]", resourceName, dependencies);

                    if (index++ < resourcesWithDependencies.Length - 1) {
                        scriptBuilder.AppendLine(",");
                    }
                }
                
                scriptBuilder.AppendFormat("}}" +
                    "}});\r\n");
            }

            var scripts = resources.Select(x => String.Format("\"{0}\"", GetModuleName(defaultSettings, appPath, x)));            
            scriptBuilder.AppendFormat("requirejs([{0}]);\r\n", String.Join("\r\n,", scripts));

            var tagBuilder = new TagBuilder("script") {
                InnerHtml = scriptBuilder.ToString()
            };
            tagBuilder.Attributes["type"] = "text/javascript";
            Output.Write(tagBuilder.ToString(TagRenderMode.Normal));
        }

        private static string GetModuleName(RequireSettings defaultSettings, string appPath, ResourceRequiredContext resource) {
            var url = resource.GetResourceUrl(defaultSettings, appPath);

            if (IsAbsoluteUrl(url))
                return url;

            var path = url.Substring(appPath.Length);
            var name = TrimStartingSlash(GetPathWithoutExtension(path).ToLowerInvariant());
            return name;
        }

        private static string TrimStartingSlash(string path) {
            return path.StartsWith("/") ? path.Substring(1) : path;
        }

        private static string GetPathWithoutExtension(string path) {
            var index = path.LastIndexOf('.');
            return index > 0 ? path.Substring(0, index) : path;
        }

        private static bool IsAbsoluteUrl(string url) {
            var segments = new[] {"http", "https", "ftp", "ftps", "//"};
            return segments.Any(url.StartsWith);
        }
    }
}