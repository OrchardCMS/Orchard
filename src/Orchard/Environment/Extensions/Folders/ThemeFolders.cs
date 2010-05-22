using System.Collections.Generic;
using Orchard.Caching;
using Orchard.FileSystems.WebSite;

namespace Orchard.Environment.Extensions.Folders {
    public class ThemeFolders : ExtensionFolders {
        public ThemeFolders(IEnumerable<string> paths, ICacheManager cacheManager, IWebSiteFolder webSiteFolder) : 
            base(paths, "Theme.txt", false/*manifestIsOptional*/, cacheManager, webSiteFolder) {
        }
    }
}