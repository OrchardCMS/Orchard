using System.Collections.Generic;
using System.Web;
using Orchard.Environment.Extensions.Models;

namespace Orchard.UI.Resources {
    public abstract class ResourceManifest : IResourceManifest {
        private string _basePath;
        private readonly IDictionary<string, IDictionary<string, ResourceDefinition>> _resources = new Dictionary<string, IDictionary<string, ResourceDefinition>>();

        public virtual Feature Feature { get; set; }

        public virtual string Name {
            get {
                return GetType().Name;
            }
        }

        public virtual ResourceDefinition DefineResource(string resourceType, string resourceName) {
            var definition = new ResourceDefinition(this, resourceType, resourceName);
            var resources = GetResources(resourceType);
            resources[resourceName] = definition;
            return definition;
        }

        protected ResourceDefinition DefineScript(string name) {
            return DefineResource("script", name);
        }

        protected ResourceDefinition DefineStyle(string name) {
            return DefineResource("stylesheet", name);
        }

        public virtual IDictionary<string, ResourceDefinition> GetResources(string resourceType) {
            IDictionary<string, ResourceDefinition> resources;
            if (!_resources.TryGetValue(resourceType, out resources)) {
                _resources[resourceType] = resources = new Dictionary<string, ResourceDefinition>();
            }
            return resources;
        }

        public string BasePath {
            get {
                if (_basePath == null && Feature != null) {
                    _basePath = VirtualPathUtility.AppendTrailingSlash(Feature.Descriptor.Extension.Location + "/" + Feature.Descriptor.Extension.Name);
                }
                return _basePath;
            }
        }
    }

    internal class DynamicResourceManifest : ResourceManifest {
    }

    public class ThemesResourceManifest : ResourceManifest {
        public ThemesResourceManifest() {
            // todo: wrong place to define all these resources.
            // But the ResourceManifest defined in Orchard.Web/themes is not loading for some reason
            DefineStyle("Admin").SetUrl("~/modules/orchard.themes/styles/admin.css");

            DefineScript("ShapesBase").SetUrl("~/core/shapes/scripts/base.js").SetDependencies("jQuery");
            DefineStyle("Shapes").SetUrl("~/core/shapes/styles/site.css"); // todo: missing
            DefineStyle("ShapesSpecial").SetUrl("~/core/shapes/styles/special.css");

            DefineStyle("Classic").SetUrl("~/themes/classic/styles/site.css");
            DefineStyle("Classic_Blog").SetUrl("~/themes/classic/styles/blog.css");

            DefineStyle("ClassicDark").SetUrl("~/themes/classicdark/styles/site.css");
            DefineStyle("ClassicDark_Blog").SetUrl("~/themes/classicdark/styles/blog.css");

            DefineStyle("Contoso").SetUrl("~/themes/contoso/styles/site.css");
            DefineStyle("Contoso_Search").SetUrl("~/themes/contoso/styles/search.css");
            
            // todo: include and define the min.js version too
            // todo: move EasySlider to common location?
            DefineScript("EasySlider").SetUrl("~/themes/contoso/scripts/easySlider.js").SetDependencies("jQuery");

            DefineStyle("Corporate").SetUrl("~/themes/corporate/styles/site.css");

            DefineStyle("Green").SetUrl("~/themes/green/styles/site.css");
            DefineStyle("Green_Blog").SetUrl("~/themes/green/styles/blog.css");
            DefineStyle("Green_YUI").SetUrl("~/themes/green/styles/yui.css");

            DefineStyle("SafeMode").SetUrl("~/themes/safemode/styles/site.css");

            DefineStyle("TheAdmin").SetUrl("~/themes/theadmin/styles/site.css");
            DefineStyle("TheAdmin_IE").SetUrl("~/themes/theadmin/styles/ie.css");
            DefineStyle("TheAdmin_IE6").SetUrl("~/themes/theadmin/styles/ie6.css");
            DefineScript("TheAdmin").SetUrl("~/themes/theadmin/scripts/admin.js").SetDependencies("jQuery");
        }
    }
}
