using Orchard.UI.Navigation;

namespace Orchard.Dashboards {
    public class AdminMenu : Component, INavigationProvider {

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Settings"), settings => settings
                .Add(T("Dashboard"), "16", dashboard => dashboard
                    .Action("Edit", "Dashboard", new { area = "Orchard.Dashboards" })));
        }
    }
}