using System.Collections.Generic;
using Orchard.Media.Models;

namespace Orchard.Media.ViewModels {
    public class MediaFolderIndexViewModel {
        public IEnumerable<MediaFolder> MediaFolders { get; set; }
    }
}
