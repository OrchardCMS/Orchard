using System.Collections.Generic;
using Orchard.Mvc.ViewModels;
using Orchard.Media.Models;

namespace Orchard.Media.ViewModels {
    public class MediaFolderIndexViewModel : AdminViewModel {
        public IEnumerable<MediaFolder> MediaFolders { get; set; }
    }
}
