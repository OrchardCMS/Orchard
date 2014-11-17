using Orchard.MediaLibrary.Models;

namespace Orchard.Layouts.ViewModels {
    public class VectorImageEditorViewModel {
        public string VectorImageId { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public VectorImagePart CurrentVectorImage { get; set; }
    }
}