using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Folders {
    public class ThemeFolders : ExtensionFolders {
        public ThemeFolders(IEnumerable<string> paths) : 
            base(paths, "Theme.txt", false/*manifestIsOptional*/) {
        }
    }
}