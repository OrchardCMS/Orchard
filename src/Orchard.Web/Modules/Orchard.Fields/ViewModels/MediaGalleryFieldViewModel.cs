using System.Collections.Generic;
using Orchard.Fields.Fields;

namespace Orchard.Fields.ViewModels {

    public class MediaGalleryFieldViewModel {

        public ICollection<MediaGalleryItem> Items { get; set; }
        public string SelectedItems { get; set; }
        public MediaGalleryField Field { get; set; }
    }
}