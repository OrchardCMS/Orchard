using Orchard.MediaLibrary.Models;

namespace Orchard.Layouts.ViewModels {
    public class VectorGraphicEditorViewModel {
        public string VectorGraphicId { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public VectorGraphicPart CurrentVectorGraphic { get; set; }
    }
}