using Orchard.MediaLibrary.Models;

namespace Orchard.Layouts.ViewModels {
    public class ImageEditorViewModel {
        public string ImageId { get; set; }
        public ImagePart CurrentImage { get; set; }
    }
}