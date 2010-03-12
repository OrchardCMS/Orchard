using System.Collections.Generic;
using Orchard.Media.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Media.ViewModels {
    public class MediaFolderEditViewModel : BaseViewModel {
        public string FolderName { get; set; }
        public string MediaPath { get; set; }
        public IEnumerable<MediaFolder> MediaFolders { get; set; }
        public IEnumerable<MediaFile> MediaFiles { get; set; }
    }
}
