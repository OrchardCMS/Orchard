using System.Collections.Generic;

namespace Orchard.Extensions {
    public class AreaFolders : ExtensionFolders {
        public AreaFolders(IEnumerable<string> paths) : 
            base(paths, "Package.txt", true/*isManifestOptional*/) {
        }
    }
}