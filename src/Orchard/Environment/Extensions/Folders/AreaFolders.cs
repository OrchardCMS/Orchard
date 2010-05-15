using System.Collections.Generic;
using Orchard.Caching;
using Orchard.Environment.FileSystems;

namespace Orchard.Environment.Extensions.Folders {
    public class AreaFolders : ExtensionFolders {
        public AreaFolders(IEnumerable<string> paths, ICacheManager cacheManager, IVirtualPathProvider virtualPathProvider) :
            base(paths, "Module.txt", true/*isManifestOptional*/, cacheManager, virtualPathProvider) {
        }
    }
}