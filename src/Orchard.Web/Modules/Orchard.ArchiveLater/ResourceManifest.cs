using Orchard.UI.Resources;

namespace Orchard.ArchiveLater {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineStyle("ArchiveLater_DatePicker").SetUrl("orchard-archivelater-datetime.css");
        }
    }
}
