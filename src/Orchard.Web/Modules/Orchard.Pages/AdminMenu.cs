using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Core.Dashboard {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("page");
        }
    }
}