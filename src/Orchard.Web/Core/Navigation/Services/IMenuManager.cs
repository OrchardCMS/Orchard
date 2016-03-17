using System.Collections.Generic;

namespace Orchard.Core.Navigation.Services {
    public interface IMenuManager : IDependency {
        
        /// <summary>
        /// Gets the list of Menu Item content types
        /// </summary>
        /// <returns>An IEnumerable{MenuItemDescriptor} containing the menu items content types.</returns>
        IEnumerable<MenuItemDescriptor> GetMenuItemTypes();
    }

    public class MenuItemDescriptor {
        public string Type { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }
}
