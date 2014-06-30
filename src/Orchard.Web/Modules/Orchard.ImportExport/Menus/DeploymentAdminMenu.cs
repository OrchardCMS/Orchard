using Orchard.Environment.Extensions;
using Orchard.ImportExport.Permissions;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.ImportExport.Menus {
    [OrchardFeature("Orchard.Deployment")]
    public class DeploymentAdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public string MenuName {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Deployment"), "42", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder menu) {
            menu.Add(T("Deployments"), "0", item => item
                .Action("Index", "Subscription", new { area = "Wng.Deployment" })
                .Permission(DeploymentPermissions.ConfigureDeployments)
                .LocalNav());
            menu.Add(T("History"), "1", item => item
                .Action("Index", "Deployment", new { area = "Wng.Deployment" })
                .Permission(DeploymentPermissions.ViewDeploymentHistory)
                .LocalNav());
            menu.Add(T("Sources and Targets"), "2", item => item
                .Action("Index", "DeploymentConfiguration", new { area = "Wng.Deployment" })
                .Permission(DeploymentPermissions.ConfigureDeployments)
                .LocalNav());
        }
    }
}
