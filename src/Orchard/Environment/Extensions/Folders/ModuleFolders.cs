using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Folders {
    public class ModuleFolders : ExtensionFolders {
        public ModuleFolders(IEnumerable<string> paths) : 
            base(paths, "Module.txt", false/*isManifestOptional*/) {
        }
    }
}