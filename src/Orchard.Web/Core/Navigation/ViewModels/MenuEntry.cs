using Orchard.ContentManagement;

namespace Orchard.Core.Navigation.ViewModels {
    public class MenuEntry {
        public int MenuId { get; set; }
        public bool IsSelected { get; set; }
        public string MenuName { get; set; }
        public ContentItem ContentItem { get; set; }
    }
}