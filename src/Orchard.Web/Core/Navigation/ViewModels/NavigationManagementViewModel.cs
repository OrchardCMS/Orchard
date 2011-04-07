using System.Collections.Generic;
using System.Linq;

namespace Orchard.Core.Navigation.ViewModels {
    public class NavigationManagementViewModel  {
        public NavigationManagementViewModel() {
            MenuItemEntries = Enumerable.Empty<MenuItemEntry>().ToList();
        }

        public MenuItemEntry NewMenuItem { get; set; }
        public IList<MenuItemEntry> MenuItemEntries { get; set; }
    }
}
