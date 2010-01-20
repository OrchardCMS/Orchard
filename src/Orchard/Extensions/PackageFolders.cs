using System.Collections.Generic;

namespace Orchard.Extensions {
    public class PackageFolders : ExtensionFolders {
        public PackageFolders(IEnumerable<string> paths) : 
            base(paths, "Package.txt", false/*isManifestOptional*/) {
        }
    }
}
