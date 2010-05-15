using System.Collections.Generic;
using Orchard.Caching;
using Orchard.Environment.FileSystems;

namespace Orchard.Environment.Extensions.Folders {
    public class ModuleFolders : ExtensionFolders {
        public ModuleFolders(IEnumerable<string> paths, ICacheManager cacheManager, IVirtualPathProvider virtualPathProvider) : 
            base(paths, "Module.txt", false/*isManifestOptional*/, cacheManager, virtualPathProvider) {
        }
    }
}