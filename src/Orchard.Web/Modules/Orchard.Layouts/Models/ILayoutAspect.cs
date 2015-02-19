using Orchard.ContentManagement;

namespace Orchard.Layouts.Models {
    public interface ILayoutAspect : IContent {
        string LayoutData { get; set; }
    }
}