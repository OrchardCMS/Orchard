using Orchard.ContentManagement;
using Orchard.MediaLibrary.Models;

namespace Orchard.ImageEditor.Models {
    public class ImageEditorPart : ContentPart {
        public MediaPart MediaPart { get; set; }
        public ImagePart ImagePart { get; set; }
    }
}