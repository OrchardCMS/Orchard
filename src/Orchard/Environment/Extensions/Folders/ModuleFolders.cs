using System.Collections.Generic;
using Orchard.Caching;
using Orchard.FileSystems.WebSite;

namespace Orchard.Environment.Extensions.Folders {
    public class ModuleFolders : ExtensionFolders {
        public ModuleFolders(IEnumerable<string> paths, ICacheManager cacheManager, IWebSiteFolder webSiteFolder) : 
            base(paths, "Module.txt", false/*isManifestOptional*/, cacheManager, webSiteFolder) {
        }
    }
}