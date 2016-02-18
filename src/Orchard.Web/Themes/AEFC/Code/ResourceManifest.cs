using Orchard.UI.Resources;

namespace AEFC
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            var manifest = builder.Add();
            manifest.DefineStyle("customer_theme").SetUrl("customer_theme.css");
        }
    }
}
