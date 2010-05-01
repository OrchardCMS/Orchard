using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Folders {
    public class AreaFolders : ExtensionFolders {
        public AreaFolders(IEnumerable<string> paths) : 
            base(paths, "Module.txt", true/*isManifestOptional*/) {
        }
    }
}