using Orchard.UI.Navigation;

namespace Orchard.DevTools {
    public class AdminMenu : INavigationProvider {
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            
        }
    }
}