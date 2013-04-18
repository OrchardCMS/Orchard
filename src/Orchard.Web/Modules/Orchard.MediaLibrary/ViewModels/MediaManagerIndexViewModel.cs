using System.Collections.Generic;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.ViewModels {
    public class MediaManagerIndexViewModel {
        public IEnumerable<MediaFolder> Folders { get; set; }
        public IEnumerable<MediaFolder> Hierarchy { get; set; }
        public int? Folder { get; set; }
        public bool DialogMode { get; set; }
    }
}