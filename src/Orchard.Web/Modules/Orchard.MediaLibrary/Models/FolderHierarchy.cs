using System.Collections.Generic;

namespace Orchard.MediaLibrary.Models {
    public class FolderHierarchy  {
        public FolderHierarchy(IMediaFolder root) {
            Root = root;
            Children = new List<FolderHierarchy>();
        }

        public IMediaFolder Root { get; set; }
        public IEnumerable<FolderHierarchy> Children { get; set; }
    }
}