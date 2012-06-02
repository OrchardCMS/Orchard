using System.Collections.Generic;
using Orchard.Core.Navigation.Models;

namespace Orchard.Core.Navigation.ViewModels {
    public class NavigationPartViewModel {
        public IEnumerable<MenuPart> ContentMenuItems { get; set; }
        public NavigationPart Part { get; set; }
    }
}