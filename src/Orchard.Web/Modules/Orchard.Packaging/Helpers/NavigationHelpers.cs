using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.Packaging.Helpers {
    public class NavigationHelpers {
        public static NavigationItemBuilder Describe(NavigationItemBuilder item, string actionName, string controllerName, bool localNav) {
            item = item.Action(actionName, controllerName, new {area = "Orchard.Packaging"}).Permission(StandardPermissions.SiteOwner);
            if (localNav)
                item = item.LocalNav();
            return item;
        }
    }
}