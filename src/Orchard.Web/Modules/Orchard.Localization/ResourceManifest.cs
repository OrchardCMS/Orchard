using Orchard.UI.Resources;

namespace Orchard.Localization {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("Flags").SetUrl("flags.css");
            manifest.DefineStyle("Localization").SetUrl("orchard-localization-base.css");
            manifest.DefineStyle("LocalizationAdmin").SetUrl("orchard-localization-admin.css");
        }
    }
}
