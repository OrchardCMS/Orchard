using Orchard.UI.Navigation;

namespace Orchard.Dashboards {
    public class AdminMenu : Component, INavigationProvider {

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Settings"), settings => settings
                .Add(T("Dashboards"), "16", dashboard => dashboard
                    .Add(T("Settings"), "5", dashboardSettings => dashboardSettings
                        .Action("Index", "Settings", new { area = "Orchard.Dashboards" })
                        .LocalNav())
                    .Add(T("Manage Dashboards"), "6", manageDashboards => manageDashboards
                        .Action("List", "Dashboard", new { area = "Orchard.Dashboards" })
                        .LocalNav())));
        }
    }
}