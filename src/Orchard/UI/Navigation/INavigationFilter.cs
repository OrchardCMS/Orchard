using System.Collections.Generic;

namespace Orchard.UI.Navigation {
    public interface INavigationFilter : IDependency {
        IEnumerable<MenuItem> Filter(IEnumerable<MenuItem> menuItems);
    }
}
