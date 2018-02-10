using System.Collections.Generic;
using Orchard.MediaPicker.Fields;

namespace Orchard.MediaPicker.ViewModels {

    public class MediaGalleryFieldViewModel {

        public ICollection<MediaGalleryItem> Items { get; set; }
        public string SelectedItems { get; set; }
        public MediaGalleryField Field { get; set; }
    }
}