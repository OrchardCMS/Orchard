using System.Collections.Generic;
using Orchard.MediaLibrary.Models;
using Orchard.UI.Navigation;

namespace Orchard.MediaLibrary.ViewModels {
    public class MediaManagerImportViewModel {
        public IEnumerable<MenuItem> Menu { get; set; }
        public IEnumerable<string> ImageSets { get; set; }
        public ICollection<MediaFolder> Hierarchy { get; set; }
    }
}