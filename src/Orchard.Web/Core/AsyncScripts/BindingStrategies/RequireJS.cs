using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Orchard.DisplayManagement.Descriptors.ResourceBindingStrategy;
using Orchard.Environment;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.VirtualPath;
using Orchard.UI.Resources;

namespace Orchard.Core.AsyncScripts.BindingStrategies {
    /// <summary>
    /// Adds another set of shapes to the shape table for script files, with a modified prefix.
    /// </summary>
    [OrchardFeature("AsyncScripts.RequireJS")]
    public class RequireJS : ScriptBindingStrategy {

        public RequireJS(IExtensionManager extensionManager, ShellDescriptor shellDescriptor, IVirtualPathProvider virtualPathProvider)
            : base(extensionManager, shellDescriptor, virtualPathProvider) {
        }

        public override string GetShapePrefix() {
            return "RequireJSAsyncScript__";
        }

        public override string GetShapeName() {
            return "RequireJSAsyncScript";
        }

        public static void WriteResource(dynamic display, TextWriter output, string url) {
            if (VirtualPathUtility.IsAppRelative(url)) {
                url = VirtualPathUtility.ToAbsolute(url);
            }
            var tagBuilder = new TagBuilder("script");
            tagBuilder.Attributes["type"] = "text/javascript";
            tagBuilder.InnerHtml = "requirejs([\"" + url + "\"]);";
            output.Write(tagBuilder.ToString(TagRenderMode.Normal));
        }

        public override void WriteResource(dynamic display, TextWriter output, ResourceDefinition resource, string url, string condition, Dictionary<string, string> attributes) {
            WriteResource(display, output, url);
        }
    }
}