using Orchard.UI.Resources;

namespace Orchard.PublishLater {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineStyle("PublishLater_DatePicker").SetUrl("orchard-publishlater-datetime.css");
        }
    }
}
