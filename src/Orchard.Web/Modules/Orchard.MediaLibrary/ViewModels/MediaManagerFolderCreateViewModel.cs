using System.Collections.Generic;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.ViewModels {
    public class MediaManagerFolderCreateViewModel {
        public string Name { get; set; }
        public int? ParentFolderId { get; set; }
        public IEnumerable<MediaFolder> Hierarchy { get; set; }
    }
}
