using System.Collections.Generic;

namespace Orchard.Core.Navigation.ViewModels {
    public class NavigationIndexViewModel {
        public IList<MenuEntry> Menus { get; set; }
        public NavigationIndexBulkAction BulkAction { get; set; }
        public int? LastMenuId { get; set; }
    }

    public enum NavigationIndexBulkAction {
        None,
        Delete
    }
}
