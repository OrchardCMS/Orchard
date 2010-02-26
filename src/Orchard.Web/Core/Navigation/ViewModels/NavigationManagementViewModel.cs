using System.Collections.Generic;
using System.Linq;
using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Navigation.ViewModels {
    public class NavigationManagementViewModel : BaseViewModel {
        public NavigationManagementViewModel() {
            MenuItemEntries = Enumerable.Empty<MenuItemEntry>().ToList();
        }

        public CreateMenuItemViewModel NewMenuItem { get; set; }
        public IList<MenuItemEntry> MenuItemEntries { get; set; }
    }
}
