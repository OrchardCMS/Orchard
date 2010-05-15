using System.Collections.Generic;
using Orchard.Caching;
using Orchard.Environment.FileSystems;

namespace Orchard.Environment.Extensions.Folders {
    public class ThemeFolders : ExtensionFolders {
        public ThemeFolders(IEnumerable<string> paths, ICacheManager cacheManager, IVirtualPathProvider virtualPathProvider) : 
            base(paths, "Theme.txt", false/*manifestIsOptional*/, cacheManager, virtualPathProvider) {
        }
    }
}