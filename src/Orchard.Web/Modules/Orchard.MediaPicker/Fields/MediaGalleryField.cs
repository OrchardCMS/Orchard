using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using Orchard.ContentManagement.FieldStorage;

namespace Orchard.MediaPicker.Fields {
    public class MediaGalleryField : ContentField {
        internal LazyField<ICollection<MediaGalleryItem>> _mediaGalleryItems = new LazyField<ICollection<MediaGalleryItem>>();

        public ICollection<MediaGalleryItem> Items { get { return _mediaGalleryItems.Value ?? new MediaGalleryItem[0]; } }

        public string SelectedItems {
            get { return Storage.Get<string>(); }
            set { Storage.Set(value); }
        }
    }

    public class MediaGalleryItem {
        public string Url { get; set; }
        public string AlternateText { get; set; }
        public string Class { get; set; }
        public string Style { get; set; }
        public string Alignment { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
