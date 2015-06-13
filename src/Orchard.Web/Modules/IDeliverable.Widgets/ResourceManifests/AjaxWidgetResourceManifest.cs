using Orchard.Environment.Extensions;
using Orchard.UI.Resources;

namespace IDeliverable.Widgets.ResourceManifests {
    [OrchardFeature("IDeliverable.Widgets.Ajax")]
    public class AjaxWidgetResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineScript("Ajaxify").SetUrl("ajaxify.js").SetDependencies("jQuery");
        }
    }
}