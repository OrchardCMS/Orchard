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

namespace Orchard.Core.AsyncScripts.BindingStrategies {
    [OrchardFeature("AsyncScripts.RequireJS")]
    public class RequireJSResourceWriter : IShapeTableProvider {
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

            var scripts = resources.Select(x => String.Format("\"{0}\"", GetUrl(defaultSettings, appPath, x)));

            var tagBuilder = new TagBuilder("script");
            tagBuilder.Attributes["type"] = "text/javascript";
            tagBuilder.InnerHtml = String.Format("requirejs([{0}])", String.Join(",", scripts));
            Output.Write(tagBuilder.ToString(TagRenderMode.Normal));
        }

        private static string GetUrl(RequireSettings defaultSettings, string appPath, ResourceRequiredContext resource) {
            var url = resource.GetResourceUrl(defaultSettings, appPath);
            if (VirtualPathUtility.IsAppRelative(url)) {
                url = VirtualPathUtility.ToAbsolute(url);
            }
            return url;
        }
    }
}