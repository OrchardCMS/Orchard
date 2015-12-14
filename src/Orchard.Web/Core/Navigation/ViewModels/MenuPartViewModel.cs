using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Core.Navigation.ViewModels {
    public class MenuPartViewModel {
        public IEnumerable<ContentItem> Menus { get; set; }
        public int CurrentMenuId { get; set; }
        public bool OnMenu { get; set; }

        public ContentItem ContentItem { get; set; } 
        public string MenuText { get; set; }
    }
}