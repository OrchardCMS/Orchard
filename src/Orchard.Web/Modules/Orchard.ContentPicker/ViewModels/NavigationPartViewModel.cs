using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentPicker.Models;
using Orchard.Core.Navigation.Models;

namespace Orchard.ContentPicker.ViewModels {
    public class NavigationPartViewModel {
        public IEnumerable<MenuPart> ContentMenuItems { get; set; }
        public NavigationPart Part { get; set; }
        public IEnumerable<ContentItem> Menus { get; set; }
        public string MenuText { get; set; }
        public bool AddMenuItem { get; set; }
        public int CurrentMenuId { get; set; }
    }
}