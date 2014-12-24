using Orchard.ContentManagement;

namespace Orchard.Layouts.ViewModels {
    public class LayoutPartViewModel {
        public IContent Content { get; set; }
        public string State { get; set; }
        public string Trash { get; set; }
        public int? TemplateId { get; set; }
    }
}