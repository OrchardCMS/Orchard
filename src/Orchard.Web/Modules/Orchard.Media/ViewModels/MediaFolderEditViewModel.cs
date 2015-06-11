using System.Collections.Generic;
using Orchard.Media.Models;

namespace Orchard.Media.ViewModels {
    public class MediaFolderEditViewModel {
        public string FolderName { get; set; }
        public string MediaPath { get; set; }
        public IEnumerable<MediaFolder> MediaFolders { get; set; }
        public IEnumerable<MediaFile> MediaFiles { get; set; }
    }
}
