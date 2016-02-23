using Orchard.UI.Resources;

namespace AEFC
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            var manifest = builder.Add();
            manifest.DefineStyle("template").SetUrl("billionthemes-2608975.css");
            manifest.DefineStyle("socicon").SetCdn("//file.myfontastic.com/n6vo44Re5QaWo8oCKShBs7/icons.css","//file.myfontastic.com/n6vo44Re5QaWo8oCKShBs7/icons.css",true);            
        }
    }
}
