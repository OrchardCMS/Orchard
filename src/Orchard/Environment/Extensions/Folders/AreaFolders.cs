using System.Collections.Generic;
using Orchard.Caching;
using Orchard.FileSystems.WebSite;

namespace Orchard.Environment.Extensions.Folders {
    public class AreaFolders : ExtensionFolders {
        public AreaFolders(IEnumerable<string> paths, ICacheManager cacheManager, IWebSiteFolder webSiteFolder) :
            base(paths, "Module.txt", true/*isManifestOptional*/, cacheManager, webSiteFolder) {
        }
    }
}