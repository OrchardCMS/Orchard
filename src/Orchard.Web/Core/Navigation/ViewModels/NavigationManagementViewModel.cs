using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Services;

namespace Orchard.Core.Navigation.ViewModels {
    public class NavigationManagementViewModel  {
        public NavigationManagementViewModel() {
            MenuItemEntries = Enumerable.Empty<MenuItemEntry>().ToList();
        }

        public MenuItemEntry NewMenuItem { get; set; }
        public IList<MenuItemEntry> MenuItemEntries { get; set; }
        public IEnumerable<MenuItemDescriptor> MenuItemDescriptors { get; set; }

        public IEnumerable<IContent> Menus { get; set; }
        public IContent CurrentMenu { get; set; }
    }
}
