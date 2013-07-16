using System.Collections.Generic;

namespace Orchard.MediaLibrary.Models {
    public class FolderHierarchy  {
        public FolderHierarchy(MediaFolder root) {
            Root = root;
            Children = new List<FolderHierarchy>();
        }

        public MediaFolder Root { get; set; }
        public IEnumerable<FolderHierarchy> Children { get; set; }
    }
}