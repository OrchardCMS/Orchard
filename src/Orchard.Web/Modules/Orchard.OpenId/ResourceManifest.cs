using Orchard.UI.Resources;

namespace Orchard.OpenId
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest.DefineStyle("TwitterAdmin").SetUrl("twitter-admin.css");
            manifest.DefineScript("TwitterAdmin").SetUrl("twitter-admin.js");
        }
    }
}