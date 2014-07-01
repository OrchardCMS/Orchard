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
            builder.AddImageSet("importexport")
                .Add(T("Import/Export"), "42", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder menu) {
            menu.Add(T("Sources and Targets"), "10", item => item
                .Action("Index", "DeploymentConfiguration", new { area = "Orchard.ImportExport" })
                .Permission(DeploymentPermissions.ConfigureDeployments)
                .LocalNav());
            menu.Add(T("Deployments"), "11", item => item
                .Action("Index", "Subscription", new { area = "Orchard.ImportExport" })
                .Permission(DeploymentPermissions.ConfigureDeployments)
                .LocalNav());
            menu.Add(T("History"), "12", item => item
                .Action("Index", "Deployment", new { area = "Orchard.ImportExport" })
                .Permission(DeploymentPermissions.ViewDeploymentHistory)
                .LocalNav());
        }
    }
}
