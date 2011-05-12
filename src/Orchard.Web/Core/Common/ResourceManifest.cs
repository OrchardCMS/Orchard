using Orchard.UI.Resources;

namespace Orchard.Core.Common {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineStyle("Common_DatePicker").SetUrl("orchard-common-datetime.css");
        }
    }
}
