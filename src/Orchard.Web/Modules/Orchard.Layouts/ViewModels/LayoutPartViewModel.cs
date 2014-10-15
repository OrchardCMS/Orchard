using Orchard.Layouts.Models;

namespace Orchard.Layouts.ViewModels {
    public class LayoutPartViewModel {
        public LayoutPart Part { get; set; }
        public string State { get; set; }
        public string Trash { get; set; }
        public int? TemplateId { get; set; }
    }
}