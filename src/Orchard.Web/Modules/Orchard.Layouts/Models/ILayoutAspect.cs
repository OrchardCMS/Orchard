using Orchard.ContentManagement;

namespace Orchard.Layouts.Models {
    public interface ILayoutAspect : IContent {
        int? TemplateId { get; set; }
        string LayoutData { get; set; }
    }
}